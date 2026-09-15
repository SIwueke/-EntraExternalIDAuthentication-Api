namespace ExternalIdDemo.Api.Interfaces;

public interface IMfaEnrollmentStore
{
    Task StoreAsync(string entraObjectId,  string secret);

    Task<string?> GetAsync(string entraObjectId);

    Task RemoveAsync(string entraObjectId);
}