using System.Net;
using Mmo.AuthServer.Network;

namespace Mmo.AuthServer;

/// <summary>
/// Interface for the client connection.
/// </summary>
public interface IConnection
{
    /// <summary>
    /// Gets address of the client.
    /// </summary>
    EndPoint? RemoteEndPoint { get; }

    /// <summary>
    /// Send a message.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task SendAsync(IServerMessage message);

    /// <summary>
    /// Receive a message.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When receive is attempted on a closed connection.</exception>
    Task<object> ReceiveAsync();
}
