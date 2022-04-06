using System.Buffers.Binary;

namespace Mmo.AuthServer.Crypt;

/// <summary>
/// Helpers for encryption.
/// </summary>
public static class CryptHelper
{
    /// <summary>
    /// Block size for some functions in this module.
    /// </summary>
    public static readonly int BlockSize = 4;

    /// <summary>
    /// Scramble the modulus for username/password encryption.
    /// </summary>
    /// <param name="modulus">Modulus.</param>
    public static void ScrambleModulus(Span<byte> modulus)
    {
        for (var i = 0; i < 4; i++)
        {
            (modulus[i], modulus[i + 77]) = (modulus[i + 77], modulus[i]);
        }

        for (var i = 0; i < 64; i++)
        {
            modulus[i] = (byte)(modulus[i] ^ modulus[i + 64]);
        }

        for (var i = 0; i < 4; i++)
        {
            modulus[i + 13] = (byte)(modulus[i + 13] ^ modulus[i + 52]);
        }

        for (var i = 0; i < 64; i++)
        {
            modulus[i + 64] = (byte)(modulus[i + 64] ^ modulus[i]);
        }
    }

    /// <summary>
    /// Scramble the initial packet from the server.
    /// </summary>
    /// <param name="buffer">Buffer with the packet.</param>
    /// <param name="key">Encryption key.</param>
    public static void ScrambleInit(Span<byte> buffer, uint key)
    {
        // Encryption
        var rounds = (buffer.Length / BlockSize) - 1;
        for (var i = 1; i < rounds; i++)
        {
            var block = BinaryPrimitives.ReadUInt32LittleEndian(buffer[(i * BlockSize) ..]);
            key += block;
            block ^= key;
            BinaryPrimitives.WriteUInt32LittleEndian(buffer[(i * BlockSize) ..], block);
        }

        // Write the key
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[(rounds * BlockSize) ..], key);
    }

    /// <summary>
    /// Calculate the checksum of the buffer.
    /// </summary>
    /// <param name="buffer">Buffer.</param>
    /// <returns>Resulting checksum.</returns>
    public static int CalculateChecksum(ReadOnlySpan<byte> buffer)
    {
        var checksum = 0;
        for (var offset = 0; offset < buffer.Length; offset += BlockSize)
        {
            var block = BinaryPrimitives.ReadInt32LittleEndian(buffer[offset..]);
            checksum ^= block;
        }

        return checksum;
    }
}