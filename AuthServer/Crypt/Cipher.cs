namespace Mmo.AuthServer.Crypt;

/// <summary>
/// Encryption and decryption of traffic.
/// </summary>
public class Cipher
{
    /// <summary>
    /// Size of a single encryption/decryption block.
    /// </summary>
    public const int BlockSize = Blowfish.BlockSize;

    private readonly Blowfish decrypt;
    private readonly Blowfish encrypt;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cipher"/> class.
    /// </summary>
    /// <param name="key">Encryption key (uses initial key if unspecified).</param>
    public Cipher(byte[]? key = null)
    {
        key ??= new byte[] { 0x6B, 0x60, 0xCB, 0x5B, 0x82, 0xCE, 0x90, 0xB1, 0xCC, 0x2B, 0x6C, 0x55, 0x6C, 0x6C, 0x6C, 0x6C };
        this.encrypt = new Blowfish(true, key);
        this.decrypt = new Blowfish(false, key);
    }

    /// <summary>
    /// Encrypt the buffer in place.
    /// </summary>
    /// <param name="buffer">Buffer with data (must be padded).</param>
    public void Encrypt(Span<byte> buffer)
    {
        for (var offset = 0; offset < buffer.Length; offset += BlockSize)
        {
            this.encrypt.ProcessBlock(buffer[offset..], buffer[offset..]);
        }
    }
}
