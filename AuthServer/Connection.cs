using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Net;
using System.Security.Cryptography;
using Mmo.AuthServer.Crypt;
using Mmo.AuthServer.Network;
using Mmo.Common.Network;

namespace Mmo.AuthServer;

/// <summary>
/// Connection for communicating with the client.
/// </summary>
public class Connection : IConnection
{
    /// <summary>
    /// Size of the buffers for receiving and sending packets.
    /// </summary>
    public static readonly int BufferSize = 1024;

    /// <summary>
    /// Size of the packet header.
    /// </summary>
    private static readonly int HeaderSize = 2;

    private static readonly byte[] EmptyBlock = new byte[] { 0, 0, 0, 0 };

    private readonly uint scrambleKey;
    private readonly IDuplexPipe pipe;
    private Cipher cipher;

    /// <summary>
    /// Initializes a new instance of the <see cref="Connection"/> class.
    /// </summary>
    /// <param name="pipe">Pipe for communication.</param>
    /// <param name="remoteEndPoint">Address of the peer.</param>
    /// <param name="scrambleKey">Additional key for encryption.</param>
    public Connection(IDuplexPipe pipe, EndPoint? remoteEndPoint = null, uint? scrambleKey = null)
    {
        this.scrambleKey = scrambleKey ?? (uint)RandomNumberGenerator.GetInt32(int.MaxValue);
        this.cipher = new Cipher();
        this.pipe = pipe;
        this.RemoteEndPoint = remoteEndPoint;
    }

    /// <inheritdoc/>
    public EndPoint? RemoteEndPoint { get; private set; }

    /// <inheritdoc/>
    public async Task SendAsync(IServerMessage message)
    {
        this.Write(message);
        await this.pipe.Output.FlushAsync();
    }

    /// <inheritdoc/>
    public async Task<object> ReceiveAsync()
    {
        // Read header
        var result = await this.pipe.Input.ReadAtLeastAsync(HeaderSize);
        var bodyLength = this.ReadHeader(result.Buffer) - HeaderSize;
        this.pipe.Input.AdvanceTo(result.Buffer.GetPosition(HeaderSize));

        // Read body
        if (bodyLength > BufferSize)
        {
            throw new InvalidOperationException("Body too long");
        }

        result = await this.pipe.Input.ReadAtLeastAsync(bodyLength);
        var message = this.ReadBody(result.Buffer, bodyLength);
        this.pipe.Input.AdvanceTo(result.Buffer.GetPosition(bodyLength));
        return message;
    }

    private void Write(IServerMessage message)
    {
        var buffer = this.pipe.Output.GetSpan(BufferSize);

        // Write body
        var body = buffer[HeaderSize..];
        var bodyLength = this.WriteBody(message, body);

        // Write header
        var header = buffer[..HeaderSize];
        this.WriteHeader(bodyLength, header);

        this.pipe.Output.Advance(bodyLength + HeaderSize);
    }

    private int WriteBody(IServerMessage message, Span<byte> body)
    {
        // Check if message changes encryption key
        byte[]? cryptKey = null;
        if (message is ICryptKey crypt)
        {
            cryptKey = crypt.CryptKey;
        }

        var writer = new PacketWriter(body);
        message.WriteTo(ref writer);

        // Padding
        var pad = writer.Length % CryptHelper.BlockSize;
        if (pad != 0)
        {
            writer.Skip(CryptHelper.BlockSize - pad);
        }

        // Checksum
        writer.Skip(CryptHelper.BlockSize);

        // Additional encryption
        if (cryptKey != null)
        {
            writer.Skip(CryptHelper.BlockSize);
            CryptHelper.ScrambleInit(writer.AsSpan(), this.scrambleKey);
        }

        // Encryption
        pad = writer.Length % Cipher.BlockSize;
        if (pad != 0)
        {
            writer.Skip(Cipher.BlockSize - pad);
        }

        this.cipher.Encrypt(writer.AsSpan());

        // Change encryption key
        if (cryptKey != null)
        {
            this.cipher = new Cipher(cryptKey);
        }

        return writer.Length;
    }

    private void WriteHeader(int bodyLength, Span<byte> header)
    {
        BinaryPrimitives.WriteInt16LittleEndian(header, (short)(bodyLength + HeaderSize));
    }

    private int ReadHeader(ReadOnlySequence<byte> buffer)
    {
        Span<byte> header = stackalloc byte[HeaderSize];
        buffer.Slice(0, header.Length).CopyTo(header);
        return BinaryPrimitives.ReadInt16LittleEndian(header);
    }

    private object ReadBody(ReadOnlySequence<byte> read, int bodyLength)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(bodyLength);
        var body = buffer.AsSpan()[..bodyLength];
        try
        {
            // Decrypt the packet
            read.Slice(0, bodyLength).CopyTo(body);
            this.cipher.Decrypt(body);

            // Remove padding
            while (body.EndsWith(EmptyBlock))
            {
                body = body[..^EmptyBlock.Length];
            }

            // Checksum
            var expected = CryptHelper.CalculateChecksum(body[..^CryptHelper.BlockSize]);
            var actual = BinaryPrimitives.ReadInt32LittleEndian(body[^CryptHelper.BlockSize..]);
            if (expected != actual)
            {
                throw new InvalidDataException();
            }

            // Read the message
            var reader = new PacketReader(body);
            return ClientMessages.ReadFrom(reader);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
