using Wallanoti.Src.Alerts.Domain.DTOs;

namespace Wallanoti.Src.Alerts.Domain.Services;

public interface INerService
{
    Task<NerEntities> ExtractEntitiesAsync(string text, CancellationToken ct = default);
}
