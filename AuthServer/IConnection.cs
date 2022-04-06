using System.Net;
using System.Runtime.Serialization;
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
    /// <exception cref="ConnectionClosedException">When receive is attempted on a closed connection.</exception>
    Task<object> ReceiveAsync();
}

/// <summary>
/// Exception thrown when the connection was closed.
/// </summary>
public class ConnectionClosedException : Exception
{
}
