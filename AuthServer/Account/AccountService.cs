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
        // Generate salt and compute the hash
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = ComputeHash(password, salt);

        // Create the record
        return this.repository.CreateAsync(username, salt, hash);
    }

    /// <inheritdoc/>
    public async Task<AccountData?> FindAsync(string username, string password)
    {
        // Fetch the account record if it exists
        var account = await this.repository.FetchAsync(username);
        if (account == null)
        {
            return null;
        }

        // Check the password
        var hash = ComputeHash(password, account.Salt);
        if (!hash.AsSpan().SequenceEqual(account.Password.AsSpan()))
        {
            return null;
        }

        return new AccountData(account.Username, account.LastWorld, account.IsBanned);
    }

    private static byte[] ComputeHash(string password, byte[] salt)
    {
        // Combine password and salt for hashing
        var encoding = Encoding.UTF8;
        var length = encoding.GetByteCount(password) + salt.Length;
        var plain = new byte[length];
        encoding.GetBytes(password, plain);
        Buffer.BlockCopy(salt, 0, plain, length - salt.Length, salt.Length);

        // Perform hashing
        return SHA256.HashData(plain);
    }
}
