using ExternalIdDemo.Api.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace ExternalIdDemo.Api.Services;

public class TotpSecretProtector : ITotpSecretProtector
{
    private readonly IDataProtector _protector;

    public TotpSecretProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector(
            "ExternalIdDemo.TotpSecret.v1");
    }

    public string Protect(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException(
                "TOTP secret cannot be empty.",
                nameof(secret));
        }

        return _protector.Protect(secret);
    }

    public string Unprotect(string protectedSecret)
    {
        if (string.IsNullOrWhiteSpace(protectedSecret))
        {
            throw new ArgumentException(
                "Protected TOTP secret cannot be empty.",
                nameof(protectedSecret));
        }

        return _protector.Unprotect(protectedSecret);
    }
}