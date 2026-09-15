using ExternalIdDemo.Api.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace ExternalIdDemo.Api.Services;

public class MfaEnrollmentStore : IMfaEnrollmentStore
{
    private readonly IDistributedCache _cache;

    public MfaEnrollmentStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task StoreAsync(string entraObjectId,  string secret)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow =
                TimeSpan.FromMinutes(10)
        };

        await _cache.SetStringAsync(GetKey(entraObjectId), secret,   options);
    }

    public Task<string?> GetAsync(string entraObjectId)
    {
        return _cache.GetStringAsync(
            GetKey(entraObjectId));
    }

    public Task RemoveAsync(string entraObjectId)
    {
        return _cache.RemoveAsync(GetKey(entraObjectId));
    }

    private static string GetKey(string entraObjectId)
    {
        return $"mfa-enrollment:{entraObjectId}";
    }
}