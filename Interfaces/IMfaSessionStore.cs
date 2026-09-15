namespace ExternalIdDemo.Api.Interfaces;

public interface IMfaSessionStore
{
    Task CreateAsync(string sessionId,  string entraObjectId, TimeSpan lifetime);

    Task<bool> IsValidAsync(string sessionId,    string entraObjectId);

    Task RemoveAsync(string sessionId);
}