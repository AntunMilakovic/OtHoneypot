using System.Net;
using System.Net.Sockets;
using OtHoneypot.Core.Interfaces;
using NModbus;
using NModbus.Extensions.Enron;
using Serilog;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace OtHoneypot.Core.Protocols;

public class Modbus : IProtocolModule
{
    private readonly IModbusConfiguration _configuration;

    public string Name { get; }
    public ProtocolType Type { get; }

    private bool State;

    private readonly ILogger _logger;
    private readonly IDataService _dataService;
    private IModbusSlave _slave;

    public Modbus(ILogger logger, IDataService dataService, IModbusConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
        _dataService = dataService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await ActLikeModbus(cancellationToken);
                await Task.Delay(1000, cancellationToken); // Simulate some work being done
            }

        }, cancellationToken);
    }

    private async Task ActLikeModbus(CancellationToken cancellationToken)
    {
        if (State == true)
            return;

        if (_configuration.Role == ProtocolRole.Master)
        {
            foreach (var device in _configuration.Devices)
            {
                foreach (var poll in device.Polls)
                    ModbusTcpMasterReadInputs(device.IPAddress, device.Port, poll.StartAddress, poll.Count);
            }
        }
        else if (_configuration.Role == ProtocolRole.Slave)
        {
            // Implement Modbus slave logic here
            // For example, you can use NModbus library to create a Modbus slave and respond to requests from Modbus masters
            StartModbusTcpSlave(cancellationToken, _configuration);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Implementation for stopping the Modbus protocol module
        return Task.CompletedTask;
    }

    public async Task ModbusTcpMasterReadInputs(string ip, int port, ushort startAddress, ushort numInputs)
    {
        try
        {
            State = true;
            using (TcpClient client = new TcpClient(ip, port))
            {
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateMaster(client);

                // read five input values

                // bool[] inputs = master.ReadInputs(0, startAddress, numInputs);
                var plcRegister = await master.ReadInputRegisters32Async(1, startAddress, numInputs);

                for (int i = 0; i < numInputs; i++)
                {
                    //logger.WriteLine($"Input {(startAddress + i)}={(inputs[i] ? 1 : 0)}");
                    _logger.Information($"Input {(startAddress + i)}={plcRegister[i]}");
                }
                State = false;
            }
        }
        catch (Exception e)
        {
            //logger.WriteLine("Error: " + e);
            Console.WriteLine("Error: " + e);
            State = false;
        }

    }

    /// <summary>
    ///     Simple Modbus TCP master read inputs example.
    /// </summary>
    public void ModbusTcpMasterReadHoldingRegisters32()
    {
        using (TcpClient client = new TcpClient("10.16.12.50", 502))
        {
            var factory = new ModbusFactory();
            IModbusMaster master = factory.CreateMaster(client);


            byte slaveId = 1;
            ushort startAddress = 7165;
            ushort numInputs = 5;
            UInt32 www = 0x42c80083;

            master.WriteSingleRegister32(slaveId, startAddress, www);
            uint[] registers = master.ReadHoldingRegisters32(slaveId, startAddress, numInputs);

            for (int i = 0; i < numInputs; i++)
            {
                Console.WriteLine($"Input {(startAddress + i)}={registers[i]}");
            }
        }
    }

    /// <summary>
    ///     Simple Modbus UDP master write coils example.
    /// </summary>
    public void ModbusUdpMasterWriteCoils()
    {
        using (UdpClient client = new UdpClient())
        {
            IPEndPoint endPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 502);
            client.Connect(endPoint);

            var factory = new ModbusFactory();

            var master = factory.CreateMaster(client);

            ushort startAddress = 1;

            // write three coils
            master.WriteMultipleCoils(0, startAddress, new bool[] { true, false, true });
        }
    }

    /// <summary>
    ///     Simple Modbus TCP slave example.
    /// </summary>
    public async Task StartModbusTcpSlave(CancellationToken cancellationToken, IModbusConfiguration configuration)
    {
        try
        {
            State = true;
            // create and start the TCP slave
            TcpListener slaveTcpListener = new TcpListener(configuration.Port);
            slaveTcpListener.Start();

            IModbusFactory factory = new ModbusFactory();

            IModbusSlaveNetwork network = factory.CreateSlaveNetwork(slaveTcpListener);

            if (configuration.Registers.Count == 0)
            {
                _logger.Error("There are no configured registers to simulate slave!");
                return;
            }

            _slave = factory.CreateSlave(1);


            var listenTask = network.ListenAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                UpdateRegisters();

                await Task.Delay(
                    TimeSpan.FromMilliseconds(1000),
                    cancellationToken);
            }

        }
        catch (Exception e)
        {
            _logger.Error("Error occured on slave: " + e.ToString());
            State = false;
        }
        finally
        {

        }
        // prevent the main thread from exiting
        // Thread.Sleep(Timeout.Infinite);

    }

    private void UpdateRegisters()
    {
        if (_slave == null)
            return;

        foreach (var register in _configuration.Registers)
        {
            if (!_dataService.GetGeneratedData(register.DataTemplateId,
                    out var value))
                continue;

            SetRegisterValue(register, value);
        }
    }

    private void SetRegisterValue(
    ModbusRegister register,
    object? value)
    {
        if (_slave == null || value == null)
            return;

        switch (register.DataType)
        {
            case ModbusDataType.UInt16:
                SetUInt16(
                    register.Type,
                    register.Address,
                    Convert.ToUInt16(value));
                break;

            case ModbusDataType.Int16:
                SetInt16(
                    register.Type,
                    register.Address,
                    Convert.ToInt16(value));
                break;

            // case ModbusDataType.UInt32:
            //     SetUInt32(
            //         register.Type,
            //         register.Address,
            //         Convert.ToUInt32(value));
            //     break;

            // case ModbusDataType.Int32:
            //     SetInt32(
            //         register.Type,
            //         register.Address,
            //         Convert.ToInt32(value));
            //     break;

            case ModbusDataType.Float32:
                SetFloat32(
                    register.Type,
                    register.Address,
                    Convert.ToSingle(value));
                break;

            default:
                throw new NotSupportedException(
                    $"Unsupported Modbus datatype: {register.DataType}");
        }
    }

    private void SetUInt16(
    ModbusRegisterType registerType,
    ushort address,
    ushort value)
    {
        if (_slave == null)
            return;

        switch (registerType)
        {
            case ModbusRegisterType.HoldingRegister:
                _slave.DataStore.HoldingRegisters.WritePoints(
                    address,
                    new[] { value });
                break;

            case ModbusRegisterType.InputRegister:
                _slave.DataStore.InputRegisters.WritePoints(
                    address,
                    new[] { value });
                break;

            default:
                throw new InvalidOperationException(
                    $"{registerType} cannot contain UInt16.");
        }
    }

    private void SetInt16(
    ModbusRegisterType registerType,
    ushort address,
    short value)
    {
        ushort rawValue = unchecked((ushort)value);

        SetUInt16(
            registerType,
            address,
            rawValue);
    }

    private void SetFloat32(
        ModbusRegisterType registerType,
        ushort address,
        float value)
    {
        uint raw = BitConverter.SingleToUInt32Bits(value);

        ushort highWord = (ushort)(raw >> 16);
        ushort lowWord = (ushort)(raw & 0xFFFF);

        WriteRegisters(
            registerType,
            address,
            new[]
            {
            highWord,
            lowWord
            });
    }

    private void WriteRegisters(
        ModbusRegisterType registerType,
        ushort address,
        ushort[] values)
    {
        if (_slave == null)
            return;

        switch (registerType)
        {
            case ModbusRegisterType.HoldingRegister:
                _slave.DataStore.HoldingRegisters.WritePoints(
                    address,
                    values);
                break;

            case ModbusRegisterType.InputRegister:
                _slave.DataStore.InputRegisters.WritePoints(
                    address,
                    values);
                break;

            default:
                throw new InvalidOperationException(
                    $"Cannot write register values to {registerType}");
        }
    }


    /// <summary>
    ///     Simple Modbus UDP slave example.
    /// </summary>
    public void StartModbusUdpSlave()
    {
        using (UdpClient client = new UdpClient(502))
        {
            var factory = new ModbusFactory();
            IModbusSlaveNetwork network = factory.CreateSlaveNetwork(client);

            IModbusSlave slave1 = factory.CreateSlave(1);
            IModbusSlave slave2 = factory.CreateSlave(2);

            network.AddSlave(slave1);
            network.AddSlave(slave2);

            network.ListenAsync().GetAwaiter().GetResult();

            // prevent the main thread from exiting
            Thread.Sleep(Timeout.Infinite);
        }
    }

    /// <summary>
    ///     Modbus TCP master and slave example.
    /// </summary>
    public void ModbusTcpMasterReadInputsFromModbusSlave()
    {
        byte slaveId = 1;
        int port = 502;
        IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });

        // create and start the TCP slave
        TcpListener slaveTcpListener = new TcpListener(address, port);
        slaveTcpListener.Start();

        var factory = new ModbusFactory();
        var network = factory.CreateSlaveNetwork(slaveTcpListener);

        IModbusSlave slave = factory.CreateSlave(slaveId);

        network.AddSlave(slave);

        var listenTask = network.ListenAsync();

        // create the master
        TcpClient masterTcpClient = new TcpClient(address.ToString(), port);
        IModbusMaster master = factory.CreateMaster(masterTcpClient);

        ushort numInputs = 5;
        ushort startAddress = 100;

        // read five register values
        ushort[] inputs = master.ReadInputRegisters(0, startAddress, numInputs);

        for (int i = 0; i < numInputs; i++)
        {
            Console.WriteLine($"Register {(startAddress + i)}={(inputs[i])}");
        }

        // clean up
        masterTcpClient.Close();
        slaveTcpListener.Stop();

        // output
        // Register 100=0
        // Register 101=0
        // Register 102=0
        // Register 103=0
        // Register 104=0
    }
}