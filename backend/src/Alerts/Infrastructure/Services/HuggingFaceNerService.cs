using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Wallanoti.Src.Alerts.Domain.DTOs;
using Wallanoti.Src.Alerts.Domain.Services;

namespace Wallanoti.Src.Alerts.Infrastructure.Services;

/// <summary>
/// Implementation of INerService that calls HuggingFace Inference API for NER.
/// </summary>
public sealed class HuggingFaceNerService : INerService
{
    private readonly HttpClient _httpClient;
    private readonly NerServiceOptions _options;
    private readonly ILogger<HuggingFaceNerService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public HuggingFaceNerService(
        HttpClient httpClient,
        NerServiceOptions options,
        ILogger<HuggingFaceNerService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<NerEntities> ExtractEntitiesAsync(string text, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _options.ModelUrl);
        request.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");

        var payload = new { inputs = text };
        var jsonContent = JsonSerializer.Serialize(payload);
        request.Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("NER service request timed out for text: {Text}", text);
            throw new TimeoutException("NER service request timed out");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "NER service request failed for text: {Text}", text);
            throw;
        }

        var responseContent = await response.Content.ReadAsStringAsync(ct);

        try
        {
            var tokens = JsonSerializer.Deserialize<List<HuggingFaceToken>>(responseContent, JsonOptions);
            return MapToNerEntities(tokens ?? []);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse NER service response: {Content}", responseContent);
            return new NerEntities(new List<string>(), null, null, null, null, null);
        }
    }

    private NerEntities MapToNerEntities(List<HuggingFaceToken> tokens)
    {
        var keywords = new List<string>();
        string? brand = null;
        decimal? maxPrice = null;
        string? location = null;

        foreach (var token in tokens)
        {
            var entity = token.Entity.ToUpperInvariant();

            if (entity.StartsWith("B-PRODUCT") || entity.StartsWith("I-PRODUCT"))
            {
                var productWord = CapitalizeFirstLetter(token.Word);
                if (!keywords.Contains(productWord))
                    keywords.Add(productWord);
            }
            else if (entity.StartsWith("B-BRAND") || entity.StartsWith("I-BRAND"))
            {
                brand = CapitalizeFirstLetter(token.Word);
            }
            else if (entity.StartsWith("B-PRICE") || entity.StartsWith("I-PRICE"))
            {
                if (decimal.TryParse(token.Word, out var price))
                    maxPrice = price;
            }
            else if (entity.StartsWith("B-LOCATION") || entity.StartsWith("I-LOCATION"))
            {
                location = CapitalizeFirstLetter(token.Word);
            }
        }

        return new NerEntities(keywords, brand, maxPrice, null, null, location);
    }

    private static string CapitalizeFirstLetter(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;
        return char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant();
    }

    private record HuggingFaceToken(string Word, int Start, int End, string Entity);
}
