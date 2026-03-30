using Microsoft.Extensions.Configuration;
using Wallanoti.Api;

namespace Wallanoti.Tests.Api;

public class CorsAllowedOriginsResolverTest
{
    [Fact]
    public void Resolve_ReturnsConfiguredOrigins_FromConfigurationSection()
    {
        var configuration = BuildConfiguration([
            new KeyValuePair<string, string?>("Cors:AllowedOrigins:0", "http://localhost:5173"),
            new KeyValuePair<string, string?>("Cors:AllowedOrigins:1", "https://app.wallanoti.com")
        ]);

        var result = CorsAllowedOriginsResolver.Resolve(configuration);

        Assert.Equal([
            "http://localhost:5173",
            "https://app.wallanoti.com"
        ], result);
    }

    [Fact]
    public void Resolve_ReturnsDistinctOrigins_WhenConfiguredThroughEnvironmentVariable()
    {
        var configuration = BuildConfiguration();

        var result = WithEnvVar("CORS_ALLOWED_ORIGINS", " http://localhost:5173 , https://app.wallanoti.com,http://localhost:5173 ",
            () => CorsAllowedOriginsResolver.Resolve(configuration));

        Assert.Equal([
            "http://localhost:5173",
            "https://app.wallanoti.com"
        ], result);
    }

    private static IConfiguration BuildConfiguration(IEnumerable<KeyValuePair<string, string?>>? values = null)
    {
        var builder = new ConfigurationBuilder();

        if (values != null)
        {
            builder.AddInMemoryCollection(values);
        }

        return builder.Build();
    }

    private static T WithEnvVar<T>(string key, string value, Func<T> action)
    {
        var originalValue = Environment.GetEnvironmentVariable(key);

        try
        {
            Environment.SetEnvironmentVariable(key, value);
            return action();
        }
        finally
        {
            Environment.SetEnvironmentVariable(key, originalValue);
        }
    }
}
