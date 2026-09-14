namespace OtHoneypot.Core.Protocols;

/// <summary>
/// Byte order used when a value spans multiple Modbus registers.
/// Letters describe the transmitted byte order of a 32-bit value.
/// </summary>
public enum ModbusByteOrder
{
    ABCD,
    BADC,
    CDAB,
    DCBA
}
