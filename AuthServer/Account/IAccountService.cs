namespace Mmo.AuthServer.Account;

/// <summary>
/// Service for managing accounts.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Create a new account.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CreateAsync(string username, string password);

    /// <summary>
    /// Find the account with the given parameters.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<AccountData?> FindAsync(string username, string password);
}

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

/// <summary>
/// Account data.
/// </summary>
/// <param name="Username">Username.</param>
/// <param name="LastWorld">Last world that the account used (or zero).</param>
/// <param name="IsBanned">Whether the account is banned.</param>
public record AccountData(string Username, byte LastWorld, bool IsBanned) { }

#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
