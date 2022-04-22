using System.Security.Cryptography;
using System.Text;

namespace Mmo.AuthServer.Account;

/// <summary>
/// Default implementation for the service managing accounts.
/// </summary>
public class AccountService : IAccountService
{
    private readonly IAccountRepository repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountService"/> class.
    /// </summary>
    /// <param name="repository">Repository for storing accounts.</param>
    public AccountService(IAccountRepository repository)
    {
        this.repository = repository;
    }

    /// <inheritdoc/>
    public Task CreateAsync(string username, string password)
    {
        // Combine password and salt for hashing
        var salt = RandomNumberGenerator.GetBytes(16);
        var encoding = Encoding.UTF8;
        var length = encoding.GetByteCount(password) + salt.Length;
        var plain = new byte[length];
        encoding.GetBytes(password, plain);
        Buffer.BlockCopy(salt, 0, plain, length - salt.Length, salt.Length);

        // Perform hashing
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(plain);

        // Create the record
        return this.repository.CreateAsync(username, salt, hash);
    }
}
