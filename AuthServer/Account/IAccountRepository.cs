namespace Mmo.AuthServer.Account;

/// <summary>
/// Repository for storing accounts.
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Create a new account.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="salt">Salt.</param>
    /// <param name="password">Password hash.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Create(string username, byte[] salt, byte[] password);

    /// <summary>
    /// Fetch a particular account.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<AccountRecord?> Fetch(string username);
}

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

/// <summary>
/// Account data stored in the repository.
/// </summary>
/// <param name="Username">Username.</param>
/// <param name="Salt">Salt.</param>
/// <param name="Password">Password hash.</param>
/// <param name="LastWorld">Last world that the account used (or zero).</param>
/// <param name="IsBanned">Whether the account is banned.</param>
public record AccountRecord(string Username, byte[] Salt, byte[] Password, short LastWorld, bool IsBanned)
{
}

#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
