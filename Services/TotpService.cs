using ExternalIdDemo.Api.Interfaces;
using OtpNet;
using System.Security.Cryptography;

namespace ExternalIdDemo.Api.Services;

public class TotpService : ITotpService
{
    private const int SecretSize = 20;

    private readonly ITotpSecretProtector _secretProtector;

    public TotpService(
        ITotpSecretProtector secretProtector)
    {
        _secretProtector = secretProtector;
    }

    /// <summary>
    /// Generates a cryptographically secure random TOTP secret.
    /// The returned value is Base32 encoded so it can be used
    /// with standard authenticator applications.
    /// </summary>
    public string GenerateSecret()
    {
        var secretBytes =
            RandomNumberGenerator.GetBytes(SecretSize);

        return Base32Encoding.ToString(secretBytes);
    }

    /// <summary>
    /// Encrypts a plaintext TOTP secret before it is
    /// persisted to SQL Server.
    /// </summary>
    public string ProtectSecret(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException(
                "TOTP secret cannot be empty.",
                nameof(secret));
        }

        return _secretProtector.Protect(secret);
    }

    /// <summary>
    /// Validates a six-digit TOTP code against an
    /// encrypted TOTP secret.
    /// </summary>
    public bool ValidateCode(
        string encryptedSecret,
        string code)
    {
        if (string.IsNullOrWhiteSpace(encryptedSecret))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        code = code.Trim();

        // TOTP codes must contain exactly six digits.
        if (code.Length != 6 ||
            !code.All(char.IsDigit))
        {
            return false;
        }

        var secret =
            _secretProtector.Unprotect(encryptedSecret);

        var secretBytes =
            Base32Encoding.ToBytes(secret);

        var totp = new Totp(secretBytes);

        return totp.VerifyTotp(
            code,
            out _,
            new VerificationWindow(
                previous: 1,
                future: 1));
    }
}