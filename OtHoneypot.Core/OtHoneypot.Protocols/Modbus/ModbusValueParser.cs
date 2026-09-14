using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace OtHoneypot.Core.Protocols;

public static class ModbusValueParser
{
    public static IReadOnlyList<ParsedModbusValue> Parse(
        ushort startAddress,
        ushort[] registers,
        ModbusDataType dataType,
        ModbusByteOrder byteOrder)
    {
        ArgumentNullException.ThrowIfNull(registers);

        if (dataType == ModbusDataType.Bool)
            throw new ArgumentException("Bool is only valid for Coil and DiscreteInput polls.", nameof(dataType));

        if (dataType == ModbusDataType.String)
        {
            var stringBytes = Reorder(ToNetworkBytes(registers), byteOrder);
            var value = Encoding.ASCII.GetString(stringBytes).TrimEnd('\0');
            return new[] { new ParsedModbusValue(startAddress, registers.Length, dataType, value) };
        }

        var registersPerValue = GetRegistersPerValue(dataType);
        if (registers.Length % registersPerValue != 0)
        {
            throw new ArgumentException(
                $"Register count {registers.Length} is not divisible by {registersPerValue} for {dataType}.",
                nameof(registers));
        }

        var result = new List<ParsedModbusValue>(registers.Length / registersPerValue);
        for (var offset = 0; offset < registers.Length; offset += registersPerValue)
        {
            var valueRegisters = registers.AsSpan(offset, registersPerValue);
            var bytes = Reorder(ToNetworkBytes(valueRegisters), byteOrder);
            result.Add(new ParsedModbusValue(
                checked((ushort)(startAddress + offset)),
                registersPerValue,
                dataType,
                ParseNumber(bytes, dataType)));
        }

        return result;
    }

    public static int GetRegistersPerValue(ModbusDataType dataType) => dataType switch
    {
        ModbusDataType.Int16 or ModbusDataType.UInt16 => 1,
        ModbusDataType.Int32 or ModbusDataType.UInt32 or ModbusDataType.Float32 => 2,
        ModbusDataType.Int64 or ModbusDataType.UInt64 or ModbusDataType.Float64 => 4,
        ModbusDataType.String => 1,
        _ => throw new NotSupportedException($"Unsupported register datatype: {dataType}")
    };

    private static object ParseNumber(byte[] bytes, ModbusDataType dataType) => dataType switch
    {
        ModbusDataType.Int16 => BinaryPrimitives.ReadInt16BigEndian(bytes),
        ModbusDataType.UInt16 => BinaryPrimitives.ReadUInt16BigEndian(bytes),
        ModbusDataType.Int32 => BinaryPrimitives.ReadInt32BigEndian(bytes),
        ModbusDataType.UInt32 => BinaryPrimitives.ReadUInt32BigEndian(bytes),
        ModbusDataType.Int64 => BinaryPrimitives.ReadInt64BigEndian(bytes),
        ModbusDataType.UInt64 => BinaryPrimitives.ReadUInt64BigEndian(bytes),
        ModbusDataType.Float32 => BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(bytes)),
        ModbusDataType.Float64 => BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(bytes)),
        _ => throw new NotSupportedException($"Unsupported register datatype: {dataType}")
    };

    private static byte[] ToNetworkBytes(ReadOnlySpan<ushort> registers)
    {
        var bytes = new byte[registers.Length * 2];
        for (var i = 0; i < registers.Length; i++)
            BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(i * 2, 2), registers[i]);
        return bytes;
    }

    private static byte[] Reorder(byte[] source, ModbusByteOrder byteOrder)
    {
        if (source.Length == 2 || byteOrder == ModbusByteOrder.ABCD)
            return source;

        var result = (byte[])source.Clone();

        if (byteOrder is ModbusByteOrder.BADC or ModbusByteOrder.DCBA)
        {
            for (var i = 0; i < result.Length; i += 2)
                (result[i], result[i + 1]) = (result[i + 1], result[i]);
        }

        if (byteOrder is ModbusByteOrder.CDAB or ModbusByteOrder.DCBA)
        {
            for (var left = 0, right = result.Length - 2; left < right; left += 2, right -= 2)
            {
                (result[left], result[right]) = (result[right], result[left]);
                (result[left + 1], result[right + 1]) = (result[right + 1], result[left + 1]);
            }
        }

        return result;
    }
}

public sealed record ParsedModbusValue(
    ushort Address,
    int RegisterCount,
    ModbusDataType DataType,
    object Value);
