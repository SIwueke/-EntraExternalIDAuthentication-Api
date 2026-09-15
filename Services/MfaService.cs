using ExternalIdDemo.Api.Data;
using ExternalIdDemo.Api.Interfaces;
using ExternalIdDemo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExternalIdDemo.Api.Services;

public class MfaService : IMfaService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITotpService _totpService;
    private readonly IMfaEnrollmentStore _enrollmentStore;
    private readonly IMfaSessionStore _sessionStore;

    public MfaService(ApplicationDbContext dbContext, ITotpService totpService,
                            IMfaEnrollmentStore enrollmentStore,  IMfaSessionStore sessionStore)
    {
        _dbContext = dbContext;
        _totpService = totpService;
        _enrollmentStore = enrollmentStore;
        _sessionStore = sessionStore;
    }

    public async Task<MfaStatusResponse> GetStatusAsync(string entraObjectId)
    {
        var userMfa = await _dbContext.UserMfas
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.EntraObjectId == entraObjectId);

        if (userMfa is null)
        {
            return new MfaStatusResponse
            {
                Enrolled = false,
                Enabled = false
            };
        }

        return new MfaStatusResponse
        {
            Enrolled = true,
            Enabled = userMfa.TotpEnabled
        };
    }

    public async Task<MfaEnrollmentResponse> StartEnrollmentAsync(string entraObjectId,  string username)
    {
        var existing = await _dbContext.UserMfas
            .SingleOrDefaultAsync(x =>
                x.EntraObjectId == entraObjectId);

        if (existing is not null && existing.TotpEnabled)
        {
            throw new InvalidOperationException(
                "TOTP MFA is already enabled for this user.");
        }

        var secret = _totpService.GenerateSecret();

        await _enrollmentStore.StoreAsync(entraObjectId,   secret);

        const string issuer = "ExternalIdDemo";

        var label = $"{issuer}:{username}";

        var otpAuthUri =
            $"otpauth://totp/{Uri.EscapeDataString(label)}" +
            $"?secret={secret}" +
            $"&issuer={Uri.EscapeDataString(issuer)}" +
            $"&algorithm=SHA1" +
            $"&digits=6" +
            $"&period=30";

        return new MfaEnrollmentResponse
        {
            Secret = secret,
            OtpAuthUri = otpAuthUri
        };
    }

    public async Task<bool> VerifyEnrollmentAsync(string entraObjectId,  string code)
    {
        var secret = await _enrollmentStore.GetAsync(entraObjectId);

        if (string.IsNullOrWhiteSpace(secret))
        {
            return false;
        }

        // Temporarily protect the secret so that the same
        // validation mechanism is used by ITotpService.
        var encryptedSecret =
            _totpService.ProtectSecret(secret);

        var valid =
            _totpService.ValidateCode(encryptedSecret,  code);

        if (!valid)
        {
            return false;
        }

        var userMfa = await _dbContext.UserMfas
            .SingleOrDefaultAsync(x =>
                x.EntraObjectId == entraObjectId);

        if (userMfa is null)
        {
            userMfa = new UserMfa
            {
                EntraObjectId = entraObjectId,
                TotpSecretEncrypted = encryptedSecret,
                TotpEnabled = true,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                LastUsedUtc = DateTime.UtcNow
            };

            _dbContext.UserMfas.Add(userMfa);
        }
        else
        {
            userMfa.TotpSecretEncrypted = encryptedSecret;
            userMfa.TotpEnabled = true;
            userMfa.UpdatedUtc = DateTime.UtcNow;
            userMfa.LastUsedUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        await _enrollmentStore.RemoveAsync(entraObjectId);

        return true;
    }
    public async Task<bool> VerifyAsync(string entraObjectId,  string code)
    {
        var userMfa = await _dbContext.UserMfas
            .SingleOrDefaultAsync(x =>
                x.EntraObjectId == entraObjectId &&
                x.TotpEnabled);

        if (userMfa is null)
        {
            return false;
        }

        var valid =  _totpService.ValidateCode(userMfa.TotpSecretEncrypted,   code);

        if (!valid)
        {
            return false;
        }

        userMfa.LastUsedUtc = DateTime.UtcNow;
        userMfa.UpdatedUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }

}