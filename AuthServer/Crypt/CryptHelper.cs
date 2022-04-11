using System.Buffers.Binary;
using System.Numerics;
using System.Security.Cryptography;

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

    /// <summary>
    /// Decrypt the credential blob received from the client.
    /// </summary>
    /// <param name="credentials">Credentials blob.</param>
    /// <param name="key">Credential key.</param>
    /// <returns>Decrypted credentials.</returns>
    /// <remarks>Decrypting manually here as RSA without padding isn't supported by cryptography library.</remarks>
    public static byte[] DecryptCredentials(byte[] credentials, RSA key)
    {
        // Extract parameters
        var parameters = key.ExportParameters(includePrivateParameters: true);
        var c = new BigInteger(
            credentials,
            isUnsigned: true,
            isBigEndian: true);
        var n = new BigInteger(
            parameters.Modulus,
            isUnsigned: true,
            isBigEndian: true);
        var d = new BigInteger(
            parameters.D,
            isUnsigned: true,
            isBigEndian: true);

        // Calculate
        BigInteger result;
        if (parameters.DP == null)
        {
            // See https://en.wikipedia.org/wiki/RSA_(cryptosystem)#Decryption
            result = BigInteger.ModPow(c, d, n);
        }
        else
        {
            // See https://en.wikipedia.org/wiki/RSA_(cryptosystem)#Using_the_Chinese_remainder_algorithm
            var dp = new BigInteger(
                parameters.DP,
                isUnsigned: true,
                isBigEndian: true);
            var dq = new BigInteger(
                parameters.DQ,
                isUnsigned: true,
                isBigEndian: true);
            var qinv = new BigInteger(
                parameters.InverseQ,
                isUnsigned: true,
                isBigEndian: true);
            var p = new BigInteger(
                parameters.P,
                isUnsigned: true,
                isBigEndian: true);
            var q = new BigInteger(
                parameters.Q,
                isUnsigned: true,
                isBigEndian: true);
            var m1 = BigInteger.ModPow(c, dp, p);
            var m2 = BigInteger.ModPow(c, dq, q);
            var h = (qinv * (m1 - m2)) % p;
            result = m2 + ((h * q) % (p * q));
        }

        // Export
        var length = result.GetByteCount(isUnsigned: true);
        var buffer = new byte[128];
        if (!result.TryWriteBytes(buffer.AsSpan()[^length..], out _, isUnsigned: true, isBigEndian: true))
        {
            throw new InvalidOperationException("Cannot export result");
        }

        return buffer;
    }
}