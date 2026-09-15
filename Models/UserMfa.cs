namespace ExternalIdDemo.Api.Models;

public class UserMfa
{
    public int Id { get; set; }

    /// <summary>
    /// The Entra External ID user's object ID (oid claim).
    /// This is the stable identifier we use to associate MFA
    /// with the authenticated Entra user.
    /// </summary>
    public string EntraObjectId { get; set; } = string.Empty;

    /// <summary>
    /// The application-owned TOTP secret, encrypted at rest.
    /// Never store the raw TOTP secret in this field.
    /// </summary>
    public string TotpSecretEncrypted { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether application TOTP MFA is enabled
    /// for this user.
    /// </summary>
    public bool TotpEnabled { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    /// <summary>
    /// Last time this user's TOTP was successfully used.
    /// Used later for replay protection/auditing.
    /// </summary>
    public DateTime? LastUsedUtc { get; set; }
}