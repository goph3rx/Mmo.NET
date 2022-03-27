namespace Mmo.AuthServer.Crypt;

/// <summary>
/// Utilities for encryption.
/// </summary>
public static class CryptUtil
{
    /// <summary>
    /// Scramble the modulus for username/password encryption.
    /// </summary>
    /// <param name="modulus">Modulus.</param>
    public static void ScrambleModulus(byte[] modulus)
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
}