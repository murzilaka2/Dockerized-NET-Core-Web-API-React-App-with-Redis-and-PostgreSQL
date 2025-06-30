namespace VotingSystem.API.Services
{
    public interface IRedisService
    {
        Task<string?> GetCachedValueAsync(string key);
        Task SetCachedValueAsync(string key, string value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task ClearAllAsync();
    }
}
