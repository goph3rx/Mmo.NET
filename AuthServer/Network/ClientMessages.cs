using Mmo.Common.Network;

namespace Mmo.AuthServer.Network;

/// <summary>
/// Implements reading messages from the client.
/// </summary>
public static class ClientMessages
{
    /// <summary>
    /// Read a message from the packet.
    /// </summary>
    /// <param name="reader">Reader with the packet.</param>
    /// <returns>Message.</returns>
    public static object ReadFrom(PacketReader reader)
    {
        var id = reader.ReadC();
        return id switch
        {
            0x00 => ReadClientRequestAuthLogin(reader),
            0x07 => new ClientAuthGameGuard(),
            _ => throw new ArgumentException($"Invalid packet id (0x{id:X2})", nameof(reader)),
        };
    }

    private static ClientRequestAuthLogin ReadClientRequestAuthLogin(PacketReader reader)
    {
        return new ClientRequestAuthLogin(reader.ReadB(128));
    }
}

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly

/// <summary>
/// Request to start GG auth.
/// </summary>
public record ClientAuthGameGuard() { }

/// <summary>
/// Request for normal auth (username/password).
/// </summary>
/// <param name="Credentials">Encrypted username and password.</param>
public record ClientRequestAuthLogin(byte[] Credentials) { }


#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
#pragma warning restore SA1009 // Closing parenthesis should be spaced correctly
