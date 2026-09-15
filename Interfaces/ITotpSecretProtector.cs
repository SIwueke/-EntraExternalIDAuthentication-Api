namespace ExternalIdDemo.Api.Interfaces;

public interface ITotpSecretProtector
{
    string Protect(string secret);

    string Unprotect(string protectedSecret);
}