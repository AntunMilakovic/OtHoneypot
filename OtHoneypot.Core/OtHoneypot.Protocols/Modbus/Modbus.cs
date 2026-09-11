using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NModbus;
using OtHoneypot.Core.Interfaces;
using Serilog;

namespace OtHoneypot.Core.Protocols;

public sealed class Modbus : IProtocolModule
{
    private readonly IModbusConfiguration _configuration;
    private readonly IDataService _dataService;
    private readonly ILogger _logger;
    private readonly Dictionary<string, ushort> _lastCommandValues = new();

    private IModbusSlave? _slave;
    private TcpListener? _slaveTcpListener;

    public string Name { get; }
    public ProtocolType Type { get; }

    public Modbus(ILogger logger, IDataService dataService, IModbusConfiguration configuration)
    {
        _logger = logger;
        _dataService = dataService;
        _configuration = configuration;
        Name = configuration.Name;
        Type = configuration.Type;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Starting Modbus module {Name}. Role: {Role}", Name, _configuration.Role);

        return _configuration.Role switch
        {
            ProtocolRole.Master => RunMasterAsync(cancellationToken),
            ProtocolRole.Slave => RunSlaveAsync(cancellationToken),
            _ => throw new NotSupportedException($"Unsupported Modbus role: {_configuration.Role}")
        };
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Information("Stopping Modbus module {Name}", Name);
        _slaveTcpListener?.Stop();
        return Task.CompletedTask;
    }

    private async Task RunMasterAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var device in _configuration.Devices)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await PollDeviceAsync(device, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error polling Modbus device {Device} at {IP}:{Port}", device.Name, device.IPAddress, device.Port);
                }
            }

            var interval = _configuration.ReadingInterval > 0 ? _configuration.ReadingInterval : 1000;
            await Task.Delay(interval, cancellationToken);
        }
    }

    private async Task PollDeviceAsync(ModbusDevice device, CancellationToken cancellationToken)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(device.IPAddress, device.Port, cancellationToken);

        var factory = new ModbusFactory();
        using var master = factory.CreateMaster(client);

        foreach (var poll in device.Polls)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadPoll(master, device, poll);
        }
    }

    private void ReadPoll(IModbusMaster master, ModbusDevice device, ModbusPollDefinition poll)
    {
        switch (poll.RegisterType)
        {
            case ModbusRegisterType.Coil:
            {
                var values = master.ReadCoils(device.UnitId, poll.StartAddress, poll.Count);
                for (var i = 0; i < values.Length; i++)
                    _logger.Information("{Device} Coil {Address} = {Value}", device.Name, poll.StartAddress + i, values[i]);
                break;
            }
            case ModbusRegisterType.DiscreteInput:
            {
                var values = master.ReadInputs(device.UnitId, poll.StartAddress, poll.Count);
                for (var i = 0; i < values.Length; i++)
                    _logger.Information("{Device} DiscreteInput {Address} = {Value}", device.Name, poll.StartAddress + i, values[i]);
                break;
            }
            case ModbusRegisterType.InputRegister:
            {
                var values = master.ReadInputRegisters(device.UnitId, poll.StartAddress, poll.Count);
                for (var i = 0; i < values.Length; i++)
                    _logger.Information("{Device} InputRegister {Address} = {Value}", device.Name, poll.StartAddress + i, values[i]);
                break;
            }
            case ModbusRegisterType.HoldingRegister:
            {
                var values = master.ReadHoldingRegisters(device.UnitId, poll.StartAddress, poll.Count);
                for (var i = 0; i < values.Length; i++)
                    _logger.Information("{Device} HoldingRegister {Address} = {Value}", device.Name, poll.StartAddress + i, values[i]);
                break;
            }
            default:
                throw new NotSupportedException($"Unsupported register type: {poll.RegisterType}");
        }
    }

    private async Task RunSlaveAsync(CancellationToken cancellationToken)
    {
        if (_configuration.Registers.Count == 0 && _configuration.CommandMappings.Count == 0)
            throw new InvalidOperationException($"Modbus slave '{Name}' has no configured registers or command mappings.");

        IPAddress bindAddress;
        if (string.IsNullOrWhiteSpace(_configuration.BindAddress) || _configuration.BindAddress == "0.0.0.0")
            bindAddress = IPAddress.Any;
        else if (!IPAddress.TryParse(_configuration.BindAddress, out bindAddress!))
            throw new InvalidOperationException($"Invalid bind address: {_configuration.BindAddress}");

        _slaveTcpListener = new TcpListener(bindAddress, _configuration.Port);
        _slaveTcpListener.Start();

        var factory = new ModbusFactory();
        var network = factory.CreateSlaveNetwork(_slaveTcpListener);

        _slave = factory.CreateSlave(_configuration.UnitId);
        network.AddSlave(_slave);

        UpdateRegisters();
        InitializeCommandValues();

        _logger.Information(
            "Modbus TCP slave {Name} listening on {Address}:{Port}, UnitId {UnitId}",
            Name,
            bindAddress,
            _configuration.Port,
            _configuration.UnitId);

        var listenTask = network.ListenAsync(cancellationToken);
        var updateTask = RunRegisterUpdateLoopAsync(cancellationToken);

        try
        {
            await Task.WhenAll(listenTask, updateTask);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            _slaveTcpListener.Stop();
            _slaveTcpListener = null;
            _slave = null;
            _logger.Information("Modbus slave {Name} stopped", Name);
        }
    }

    private async Task RunRegisterUpdateLoopAsync(CancellationToken cancellationToken)
    {
        var interval = _configuration.ReadingInterval > 0 ? _configuration.ReadingInterval : 250;

        while (!cancellationToken.IsCancellationRequested)
        {
            ProcessCommandMappings();
            UpdateRegisters();
            await Task.Delay(interval, cancellationToken);
        }
    }

    private void InitializeCommandValues()
    {
        if (_slave == null)
            return;

        foreach (var group in GetCommandGroups())
            _lastCommandValues[group.Key] = ReadCommandValue(group.First());
    }

    private void ProcessCommandMappings()
    {
        if (_slave == null)
            return;

        foreach (var group in GetCommandGroups())
        {
            var firstMapping = group.First();

            try
            {
                var currentValue = ReadCommandValue(firstMapping);

                if (_lastCommandValues.TryGetValue(group.Key, out var previousValue) && previousValue == currentValue)
                    continue;

                _lastCommandValues[group.Key] = currentValue;

                var mapping = group.FirstOrDefault(candidate => candidate.Value == currentValue);
                if (mapping == null)
                {
                    _logger.Debug(
                        "No Modbus command mapped for {RegisterType} {Address} value {Value}",
                        firstMapping.RegisterType,
                        firstMapping.Address,
                        currentValue);
                    continue;
                }

                if (!_dataService.ExecuteSimulationCommand(mapping.DataTemplateId, mapping.Command))
                {
                    _logger.Warning(
                        "Modbus command {CommandName} targets unknown DataTemplateId {DataTemplateId}",
                        mapping.Name,
                        mapping.DataTemplateId);
                    continue;
                }

                _logger.Information(
                    "Modbus command {CommandName} ({Command}) received at {RegisterType} {Address} with value {Value}",
                    mapping.Name,
                    mapping.Command,
                    mapping.RegisterType,
                    mapping.Address,
                    currentValue);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Unable to process Modbus command mapping at {RegisterType} {Address}",
                    firstMapping.RegisterType,
                    firstMapping.Address);
            }
        }
    }

    private IEnumerable<IGrouping<string, ModbusCommandMapping>> GetCommandGroups() =>
        _configuration.CommandMappings.GroupBy(GetCommandKey);

    private ushort ReadCommandValue(ModbusCommandMapping mapping)
    {
        if (_slave == null)
            return 0;

        return mapping.RegisterType switch
        {
            ModbusRegisterType.HoldingRegister => _slave.DataStore.HoldingRegisters.ReadPoints(mapping.Address, 1)[0],
            ModbusRegisterType.Coil => (ushort)(_slave.DataStore.CoilDiscretes.ReadPoints(mapping.Address, 1)[0] ? 1 : 0),
            _ => throw new InvalidOperationException("Command mappings can only use writable HoldingRegister or Coil addresses.")
        };
    }

    private static string GetCommandKey(ModbusCommandMapping mapping) =>
        $"{mapping.RegisterType}:{mapping.Address}";

    private void UpdateRegisters()
    {
        if (_slave == null)
            return;

        foreach (var register in _configuration.Registers)
        {
            if (!_dataService.GetGeneratedData(register.DataTemplateId, out var data))
            {
                _logger.Warning(
                    "No generated data found for Modbus register {Register} at address {Address}. DataTemplateId {DataTemplateId} does not exist.",
                    register.Name,
                    register.Address,
                    register.DataTemplateId);
                continue;
            }

            try
            {
                SetRegisterValue(register, data.Value);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to set Modbus register {Register} at address {Address}", register.Name, register.Address);
            }
        }
    }

    private void SetRegisterValue(ModbusRegister register, object? value)
    {
        if (_slave == null || value == null)
            return;

        if (register.Type is ModbusRegisterType.Coil or ModbusRegisterType.DiscreteInput)
        {
            if (register.DataType != ModbusDataType.Bool)
                throw new InvalidOperationException($"{register.Name}: {register.Type} must use Bool datatype.");

            SetBit(register.Type, register.Address, Convert.ToBoolean(value));
            return;
        }

        switch (register.DataType)
        {
            case ModbusDataType.UInt16:
                SetUInt16(register.Type, register.Address, Convert.ToUInt16(value));
                break;
            case ModbusDataType.Int16:
                SetInt16(register.Type, register.Address, Convert.ToInt16(value));
                break;
            case ModbusDataType.UInt32:
                SetUInt32(register.Type, register.Address, Convert.ToUInt32(value));
                break;
            case ModbusDataType.Int32:
                SetInt32(register.Type, register.Address, Convert.ToInt32(value));
                break;
            case ModbusDataType.Float32:
                SetFloat32(register.Type, register.Address, Convert.ToSingle(value));
                break;
            default:
                throw new NotSupportedException($"Unsupported Modbus datatype: {register.DataType}");
        }
    }

    private void SetBit(ModbusRegisterType registerType, ushort address, bool value)
    {
        if (_slave == null)
            return;

        switch (registerType)
        {
            case ModbusRegisterType.Coil:
                _slave.DataStore.CoilDiscretes.WritePoints(address, new[] { value });
                break;
            case ModbusRegisterType.DiscreteInput:
                _slave.DataStore.CoilInputs.WritePoints(address, new[] { value });
                break;
            default:
                throw new InvalidOperationException($"{registerType} is not a bit register.");
        }
    }

    private void SetUInt16(ModbusRegisterType registerType, ushort address, ushort value) =>
        WriteRegisters(registerType, address, new[] { value });

    private void SetInt16(ModbusRegisterType registerType, ushort address, short value) =>
        WriteRegisters(registerType, address, new[] { unchecked((ushort)value) });

    private void SetUInt32(ModbusRegisterType registerType, ushort address, uint value)
    {
        var highWord = (ushort)(value >> 16);
        var lowWord = (ushort)(value & 0xFFFF);
        WriteRegisters(registerType, address, new[] { highWord, lowWord });
    }

    private void SetInt32(ModbusRegisterType registerType, ushort address, int value) =>
        SetUInt32(registerType, address, unchecked((uint)value));

    private void SetFloat32(ModbusRegisterType registerType, ushort address, float value) =>
        SetUInt32(registerType, address, BitConverter.SingleToUInt32Bits(value));

    private void WriteRegisters(ModbusRegisterType registerType, ushort address, ushort[] values)
    {
        if (_slave == null)
            return;

        switch (registerType)
        {
            case ModbusRegisterType.HoldingRegister:
                _slave.DataStore.HoldingRegisters.WritePoints(address, values);
                break;
            case ModbusRegisterType.InputRegister:
                _slave.DataStore.InputRegisters.WritePoints(address, values);
                break;
            default:
                throw new InvalidOperationException($"{registerType} is not a 16-bit Modbus register type.");
        }
    }
}
