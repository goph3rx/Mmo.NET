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
}
