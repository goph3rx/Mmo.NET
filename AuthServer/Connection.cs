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

    private readonly uint scrambleKey;
    private readonly Cipher cipher;
    private readonly IDuplexPipe pipe;

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
        var writer = new PacketWriter(body);
        message.WriteTo(ref writer);

        // Padding
        var pad = writer.Length % CryptUtil.BlockSize;
        if (pad != 0)
        {
            writer.Skip(CryptUtil.BlockSize - pad);
        }

        // Checksum
        writer.Skip(CryptUtil.BlockSize);

        // Additional encryption
        if (message is ICryptKey)
        {
            writer.Skip(CryptUtil.BlockSize);
            CryptUtil.ScrambleInit(writer.AsSpan(), this.scrambleKey);
        }

        // Encryption
        pad = writer.Length % Cipher.BlockSize;
        if (pad != 0)
        {
            writer.Skip(Cipher.BlockSize - pad);
        }

        this.cipher.Encrypt(writer.AsSpan());

        return writer.Length;
    }

    private void WriteHeader(int bodyLength, Span<byte> header)
    {
        var writer = new PacketWriter(header);
        writer.WriteH((short)(bodyLength + HeaderSize));
    }
}
