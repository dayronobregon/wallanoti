using Wallanoti.Src.Alerts.Domain.DTOs;

namespace Wallanoti.Src.Alerts.Domain.Services;

public interface IWallapopUrlBuilder
{
    string BuildUrl(NerEntities entities);
}
