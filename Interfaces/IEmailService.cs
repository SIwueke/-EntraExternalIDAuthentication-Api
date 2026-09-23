namespace ExternalIdDemo.Api.Interfaces;

public interface IEmailService
{
    Task SendMfaCodeAsync(
        string email,
        string code);
}