using Microsoft.AspNetCore.DataProtection;

namespace AssignmentFinder.Infrastructure.Scanning;

/// <summary>Encrypts/decrypts Broker.LoginPasswordEncrypted so broker login credentials are never stored as plaintext.</summary>
public class BrokerCredentialProtector(IDataProtectionProvider provider)
{
    private readonly IDataProtector protector = provider.CreateProtector("AssignmentFinder.BrokerCredentials.v1");

    public string Encrypt(string plaintext) => protector.Protect(plaintext);

    public string Decrypt(string ciphertext) => protector.Unprotect(ciphertext);
}
