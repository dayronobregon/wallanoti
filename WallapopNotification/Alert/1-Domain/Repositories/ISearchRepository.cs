namespace WallapopNotification.Alert._1_Domain.Repositories;

public interface ISearchRepository
{
    public Task Add(Models.SearchModel searchModel);
    public Task<Models.SearchModel?> LastByUserId(Guid userId);
}