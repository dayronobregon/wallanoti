using System.Net;
using System.Net.Sockets;

namespace Wallanoti.Api;

public static class ForwardedHeadersNetworkParser
{
    public static Microsoft.AspNetCore.HttpOverrides.IPNetwork? TryParseNetwork(string cidr)
    {
        var parts = cidr.Split('/');
        if (parts.Length != 2)
        {
            return null;
        }

        if (!IPAddress.TryParse(parts[0], out var prefix) || !int.TryParse(parts[1], out var prefixLength))
        {
            return null;
        }

        var maxPrefixLength = prefix.AddressFamily switch
        {
            AddressFamily.InterNetwork => 32,
            AddressFamily.InterNetworkV6 => 128,
            _ => -1
        };

        if (prefixLength < 0 || prefixLength > maxPrefixLength)
        {
            return null;
        }

        return new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, prefixLength);
    }
}
