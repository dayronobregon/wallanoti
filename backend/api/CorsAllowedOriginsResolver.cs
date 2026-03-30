using Microsoft.Extensions.Configuration;

namespace Wallanoti.Api;

public static class CorsAllowedOriginsResolver
{
    private const string CustomEnvVar = "CORS_ALLOWED_ORIGINS";

    public static string[] Resolve(IConfiguration configuration)
    {
        var origins = new List<string>();

        var customEnvOrigins = Environment.GetEnvironmentVariable(CustomEnvVar);
        origins.AddRange(ParseOrigins(customEnvOrigins));

        var sectionOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        origins.AddRange(sectionOrigins.SelectMany(ParseOrigins));

        var csvOrigins = configuration["Cors:AllowedOrigins"];
        origins.AddRange(ParseOrigins(csvOrigins));

        return origins
            .Select(origin => origin.Trim())
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IEnumerable<string> ParseOrigins(string? rawOrigins)
    {
        if (string.IsNullOrWhiteSpace(rawOrigins))
        {
            return [];
        }

        return rawOrigins
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}
