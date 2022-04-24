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
            "Host=127.0.0.1;Username=accounts;Password=changeme;Database=accounts;Minimum Pool Size=5");
    }

    /// <inheritdoc/>
    public async Task CreateAsync(string username, byte[] salt, byte[] password)
    {
        await using var conn = new NpgsqlConnection(this.connString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("INSERT INTO accounts (username, salt, password) VALUES (@username, @salt, @password)", conn);
        cmd.Parameters.AddWithValue("username", username);
        cmd.Parameters.AddWithValue("salt", Convert.ToHexString(salt));
        cmd.Parameters.AddWithValue("password", Convert.ToHexString(password));
        cmd.Prepare();

        await cmd.ExecuteNonQueryAsync();
    }

    /// <inheritdoc/>
    public async Task<AccountRecord?> FetchAsync(string username)
    {
        await using var conn = new NpgsqlConnection(this.connString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT salt, password, last_world, is_banned FROM accounts WHERE username = @username LIMIT 1", conn);
        cmd.Parameters.AddWithValue("username", username);
        cmd.Prepare();

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!reader.HasRows)
        {
            return null;
        }

        await reader.ReadAsync();
        return new AccountRecord(
            Username: username,
            Salt: Convert.FromHexString(reader.GetString(0)),
            Password: Convert.FromHexString(reader.GetString(1)),
            LastWorld: reader.GetByte(2),
            IsBanned: reader.GetBoolean(3));
    }
}
