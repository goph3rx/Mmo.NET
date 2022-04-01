using System.Security.Cryptography;
using Mmo.AuthServer.Network;

namespace Mmo.AuthServer;

/// <summary>
/// Represents a client connected to the auth server.
/// </summary>
public class Client
{
    private readonly RSA credentialKey;
    private readonly IConnection connection;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="connection">Underlying connection.</param>
    /// <param name="logger">Logger for the class.</param>
    public Client(IConnection connection, ILogger<Client> logger)
    {
        this.credentialKey = RSA.Create(1024);
        this.connection = connection;
        this.logger = logger;
    }

    /// <summary>
    /// Initialize the client.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InitAsync()
    {
        this.logger.LogInformation("New connection from {}", this.connection.RemoteEndPoint);

        // Init the connection
        var sessionId = unchecked((int)0xDEADBEEF);
        var cryptKey = RandomNumberGenerator.GetBytes(16);
        var credentialParameters = this.credentialKey.ExportParameters(false);
        if (credentialParameters.Modulus == null)
        {
            throw new InvalidOperationException("Cannot export modulus");
        }

        // Send the initial message to the client
        var message = new ServerInit(sessionId, credentialParameters.Modulus, cryptKey);
        await this.SendAsync(message);
    }

    /// <summary>
    /// Process the commands from the client.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task RunAsync()
    {
        // Will contain a receive loop
        return Task.Delay(10000);
    }

    private Task SendAsync(IServerMessage message)
    {
        this.logger.LogDebug("Sending {}", message);
        return this.connection.SendAsync(message);
    }
}