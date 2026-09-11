using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace OtHoneypot.Core.Protocols;

internal static class IpAddressMatcher
{
    public static bool IsAllowed(IPAddress address, IEnumerable<string> allowedEntries)
    {
        address = Normalize(address);

        foreach (var entry in allowedEntries)
        {
            if (string.IsNullOrWhiteSpace(entry))
                continue;

            if (Matches(address, entry.Trim()))
                return true;
        }

        return false;
    }

    public static bool IsValidRule(string rule)
    {
        if (string.IsNullOrWhiteSpace(rule))
            return false;

        return TryParseRule(rule.Trim(), out _, out _);
    }

    private static bool Matches(IPAddress address, string rule)
    {
        if (!TryParseRule(rule, out var network, out var prefixLength))
            return false;

        address = Normalize(address);
        network = Normalize(network);

        if (address.AddressFamily != network.AddressFamily)
            return false;

        var addressBytes = address.GetAddressBytes();
        var networkBytes = network.GetAddressBytes();
        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;

        for (var i = 0; i < fullBytes; i++)
        {
            if (addressBytes[i] != networkBytes[i])
                return false;
        }

        if (remainingBits == 0)
            return true;

        var mask = (byte)(0xFF << (8 - remainingBits));
        return (addressBytes[fullBytes] & mask) == (networkBytes[fullBytes] & mask);
    }

    private static bool TryParseRule(string rule, out IPAddress address, out int prefixLength)
    {
        address = IPAddress.None;
        prefixLength = 0;

        var parts = rule.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length is < 1 or > 2 || !IPAddress.TryParse(parts[0], out var parsedAddress))
            return false;

        parsedAddress = Normalize(parsedAddress);
        var maxPrefixLength = parsedAddress.AddressFamily == AddressFamily.InterNetwork ? 32 : 128;

        if (parts.Length == 1)
        {
            address = parsedAddress;
            prefixLength = maxPrefixLength;
            return true;
        }

        if (!int.TryParse(parts[1], out prefixLength) || prefixLength < 0 || prefixLength > maxPrefixLength)
            return false;

        address = parsedAddress;
        return true;
    }

    private static IPAddress Normalize(IPAddress address) =>
        address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
}
