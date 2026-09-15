using ExternalIdDemo.Api.Models;

namespace ExternalIdDemo.Api.Interfaces;

public interface IMfaService
{
    Task<MfaStatusResponse> GetStatusAsync(string entraObjectId);

    Task<MfaEnrollmentResponse> StartEnrollmentAsync(string entraObjectId, string username);

    Task<bool> VerifyEnrollmentAsync(string entraObjectId, string code);

    Task<bool> VerifyAsync(string entraObjectId,    string code);
}