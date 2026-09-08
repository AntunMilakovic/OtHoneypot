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

    private readonly ILogger _logger;

    public Modbus(ILogger logger, IModbusConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var stopwatch = DateTime.UtcNow;
                await ActLikeModbus(cancellationToken);
                // Implement the Modbus protocol logic here
                // For example, you can listen for incoming Modbus requests and respond accordingly
                // logger.Write("Reading finished in " + DateTime.UtcNow.Subtract(stopwatch).Seconds);
                var waitSeconds = _configuration.ReadingInterval - DateTime.UtcNow.Subtract(stopwatch).Seconds;

                if (waitSeconds < 0)
                    continue;

                await Task.Delay(1000, cancellationToken); // Simulate some work being done
            }

        }, cancellationToken);
    }

    public async Task ActLikeModbus(CancellationToken cancellationToken)
    {
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
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Implementation for stopping the Modbus protocol module
        return Task.CompletedTask;
    }

    public void ModbusTcpMasterReadInputs(string ip, int port, ushort startAddress, ushort numInputs)
    {
        try
        {
            using (TcpClient client = new TcpClient(ip, port))
            {
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateMaster(client);

                // read five input values

                bool[] inputs = master.ReadInputs(0, startAddress, numInputs);

                for (int i = 0; i < numInputs; i++)
                {
                    //logger.WriteLine($"Input {(startAddress + i)}={(inputs[i] ? 1 : 0)}");
                    _logger.Information($"Input {(startAddress + i)}={(inputs[i] ? 1 : 0)}");
                }
            }
        }
        catch (Exception e)
        {
            //logger.WriteLine("Error: " + e);
            Console.WriteLine("Error: " + e);
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
    public void StartModbusTcpSlave()
    {
        int port = 502;
        IPAddress address = new IPAddress(new byte[] { 127, 0, 0, 1 });

        // create and start the TCP slave
        TcpListener slaveTcpListener = new TcpListener(address, port);
        slaveTcpListener.Start();

        IModbusFactory factory = new ModbusFactory();

        IModbusSlaveNetwork network = factory.CreateSlaveNetwork(slaveTcpListener);

        IModbusSlave slave1 = factory.CreateSlave(1);
        IModbusSlave slave2 = factory.CreateSlave(2);

        network.AddSlave(slave1);
        network.AddSlave(slave2);

        network.ListenAsync().GetAwaiter().GetResult();

        // prevent the main thread from exiting
        Thread.Sleep(Timeout.Infinite);
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