using Wallanoti.Api;

namespace Wallanoti.Tests.Api;

public class ForwardedHeadersNetworkParserTest
{
    [Theory]
    [InlineData("10.0.0.0/33")]
    [InlineData("10.0.0.0/-1")]
    [InlineData("2001:db8::/129")]
    [InlineData("2001:db8::/-1")]
    public void TryParseNetwork_ReturnsNull_WhenPrefixLengthIsOutOfRange(string cidr)
    {
        var result = ForwardedHeadersNetworkParser.TryParseNetwork(cidr);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("10.0.0.0/0")]
    [InlineData("10.0.0.0/32")]
    [InlineData("2001:db8::/0")]
    [InlineData("2001:db8::/128")]
    public void TryParseNetwork_ReturnsNetwork_WhenPrefixLengthIsInRange(string cidr)
    {
        var result = ForwardedHeadersNetworkParser.TryParseNetwork(cidr);

        Assert.NotNull(result);
    }
}
