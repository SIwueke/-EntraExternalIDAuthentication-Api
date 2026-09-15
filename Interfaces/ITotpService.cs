namespace ExternalIdDemo.Api.Interfaces;

public interface ITotpService
{
    /// <summary>
    /// Generates a new random TOTP secret.
    /// </summary>
    string GenerateSecret();

    /// <summary>
    /// Encrypts a TOTP secret for storage.
    /// </summary>
    string ProtectSecret(string secret);

    /// <summary>
    /// Validates a TOTP code against an encrypted secret.
    /// </summary>
    bool ValidateCode(
        string encryptedSecret,
        string code);
}