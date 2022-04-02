using System.Diagnostics;
using Microsoft.AspNetCore.Connections;

namespace Mmo.AuthServer;

/// <summary>
/// Accepts new connections to the server.
/// </summary>
public class Acceptor : ConnectionHandler
{
    private readonly ILogger logger;
    private readonly IServiceProvider services;

    /// <summary>
    /// Initializes a new instance of the <see cref="Acceptor"/> class.
    /// </summary>
    /// <param name="logger">Logger for this class.</param>
    /// <param name="services">Service container.</param>
    public Acceptor(ILogger<Acceptor> logger, IServiceProvider services)
    {
        this.logger = logger;
        this.services = services;
    }

    /// <inheritdoc/>
    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        // Init client
        var stopwatch = Stopwatch.StartNew();
        var client = ActivatorUtilities.CreateInstance<Client>(
            this.services,
            new Connection(connection.Transport, connection.RemoteEndPoint));
        await client.InitAsync();
        stopwatch.Stop();
        this.logger.LogTrace("Accepted new connection in {}ms", stopwatch.ElapsedMilliseconds);

        // Process the client
        await client.RunAsync();
    }
}