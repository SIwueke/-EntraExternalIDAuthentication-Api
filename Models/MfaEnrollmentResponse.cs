namespace ExternalIdDemo.Api.Models;

public class MfaEnrollmentResponse
{
    public string Secret { get; set; } = string.Empty;

    public string OtpAuthUri { get; set; } = string.Empty;
}