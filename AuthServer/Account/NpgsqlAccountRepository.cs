using Dapper;
using Npgsql;

namespace Mmo.AuthServer.Account;

/// <summary>
/// Account repository with PostgreSQL as a backend.
/// </summary>
public class NpgsqlAccountRepository : IAccountRepository
{
    private readonly string connString;

    /// <summary>
    /// Initializes a new instance of the <see cref="NpgsqlAccountRepository"/> class.
    /// </summary>
    /// <param name="configuration">Application configuration.</param>
    public NpgsqlAccountRepository(IConfiguration configuration)
    {
        this.connString = configuration.GetValue(
            "AuthServer:AccountRepository",
            "Host=127.0.0.1;Username=accounts;Password=changeme;Database=accounts");
    }

    /// <inheritdoc/>
    public async Task Create(string username, byte[] salt, byte[] password)
    {
        await using var conn = new NpgsqlConnection(this.connString);
        await conn.OpenAsync();
        await conn.ExecuteAsync(
            "INSERT INTO accounts (username, salt, password) VALUES (@username, @salt, @password)",
            new
            {
                username,
                salt = Convert.ToHexString(salt),
                password = Convert.ToHexString(password),
            });
    }

    /// <inheritdoc/>
    public async Task<AccountRecord?> Fetch(string username)
    {
        await using var conn = new NpgsqlConnection(this.connString);
        await conn.OpenAsync();
        var record = await conn.QueryFirstOrDefaultAsync(
            "SELECT salt, password, last_world, is_banned FROM accounts WHERE username = @username LIMIT 1",
            new { username });
        if (record == null)
        {
            return null;
        }

        return new AccountRecord(
            Username: username,
            Salt: Convert.FromHexString(record.salt),
            Password: Convert.FromHexString(record.password),
            LastWorld: record.last_world,
            IsBanned: record.is_banned);
    }
}
