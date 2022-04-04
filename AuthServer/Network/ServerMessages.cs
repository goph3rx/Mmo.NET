using Mmo.AuthServer.Crypt;
using Mmo.Common.Network;

namespace Mmo.AuthServer.Network;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly

/// <summary>
/// Interface for all messages from the server.
/// </summary>
public interface IServerMessage
{
    /// <summary>
    /// Write the message.
    /// </summary>
    /// <param name="writer">Writer to use.</param>
    void WriteTo(ref PacketWriter writer);
}

/// <summary>
/// Interface for a message that changes the encryption key.
/// </summary>
public interface ICryptKey
{
    /// <summary>
    /// Gets the key for traffic encryption.
    /// </summary>
    byte[] CryptKey { get; }
}

/// <summary>
/// First message from the server. Starts the auth process.
/// </summary>
/// <param name="SessionId">Session identifier.</param>
/// <param name="Modulus">Modulus for username/password encryption.</param>
/// <param name="CryptKey">Key for traffic encryption.</param>
public record ServerInit(int SessionId, byte[] Modulus, byte[] CryptKey) : ICryptKey, IServerMessage
{
    /// <summary>
    /// Version of the implemented protocol.
    /// </summary>
    private const int ProtocolVersion = 0xC621;

    /// <inheritdoc/>
    public void WriteTo(ref PacketWriter writer)
    {
        CryptHelper.ScrambleModulus(this.Modulus);

        writer.WriteC(0x00);
        writer.WriteD(this.SessionId);
        writer.WriteD(ProtocolVersion);
        writer.WriteB(this.Modulus);
        writer.Skip(16);
        writer.WriteB(this.CryptKey);
    }
}

#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
#pragma warning restore SA1009 // Closing parenthesis should be spaced correctly
