using ExternalIdDemo.Api.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace ExternalIdDemo.Api.Services;

public class MfaSessionStore : IMfaSessionStore
{
    private readonly IDistributedCache _cache;

    public MfaSessionStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task CreateAsync(string sessionId, string entraObjectId,    TimeSpan lifetime)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = lifetime
        };

        await _cache.SetStringAsync(GetKey(sessionId),  entraObjectId,   options);
    }

    public async Task<bool> IsValidAsync(string sessionId, string entraObjectId)
    {
        if (string.IsNullOrWhiteSpace(sessionId) ||
            string.IsNullOrWhiteSpace(entraObjectId))
        {
            return false;
        }

        var storedObjectId =  await _cache.GetStringAsync(GetKey(sessionId));

        return string.Equals(
            storedObjectId,
            entraObjectId,
            StringComparison.Ordinal);
    }

    public Task RemoveAsync(string sessionId)
    {
        return _cache.RemoveAsync(
            GetKey(sessionId));
    }

    private static string GetKey(
        string sessionId)
    {
        return $"mfa-session:{sessionId}";
    }
}