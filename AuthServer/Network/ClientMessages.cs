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
    public static object Read(PacketReader reader)
    {
        var id = reader.ReadC();
        return id switch
        {
            0x07 => new ClientAuthGameGuard(),
            _ => throw new ArgumentException($"Invalid packet id (0x{id:X2})", nameof(reader)),
        };
    }
}

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly

/// <summary>
/// Request to start GG auth.
/// </summary>
public record ClientAuthGameGuard() { }


#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
#pragma warning restore SA1009 // Closing parenthesis should be spaced correctly
