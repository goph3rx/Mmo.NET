using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mmo.AuthServer;
using Mmo.AuthServer.Network;
using Mmo.Common.Network;
using Moq;

namespace Mmo.Tests.Network;

[TestClass]
public class AuthConnectionTest
{
    class FakeServerMessage : ICryptKey, IServerMessage
    {
        public byte[] CryptKey => new byte[4];

        public void WriteTo(ref PacketWriter writer)
        {
            for (byte i = 1; i <= 16; i++)
            {
                writer.WriteC(i);
            }
        }
    }

    private EndPoint endPoint;
    private PipeReader reader;
    private PipeWriter writer;
    private Connection connection;

    public AuthConnectionTest()
    {
        {
            var pipe = new Pipe();
            this.reader = pipe.Reader;
            this.writer = pipe.Writer;
        }
        {
            var pipe = new Mock<IDuplexPipe>();
            pipe.Setup(x => x.Input).Returns(this.reader);
            pipe.Setup(x => x.Output).Returns(this.writer);
            this.endPoint = new IPEndPoint(0x7F000001, 8123);
            this.connection = new Connection(pipe.Object, this.endPoint, 0xD5906CBC);
        }
    }

    [TestMethod]
    public void RemoteEndPoint()
    {
        // When/Then
        Assert.AreSame(endPoint, connection.RemoteEndPoint);
    }

    [TestMethod]
    public async Task SendAsync()
    {
        // Given
        var message = new FakeServerMessage();

        // When
        await connection.SendAsync(message);

        // Then
        var result = await this.reader.ReadAtLeastAsync(26);
        var buffer = new byte[26];
        result.Buffer.CopyTo(buffer);
        Assert.AreEqual(
            "1A009BB6D905778ECB0ECF113FD58BFB83A5E8D00DA1A97F42CD",
            Convert.ToHexString(buffer)
        );
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public async Task ReceiveAsyncHeaderClosed()
    {
        // Given
        this.writer.Complete();

        // When/Then
        await connection.ReceiveAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public async Task ReceiveAsyncHeaderTooSmall()
    {
        // Given
        await this.writer.WriteAsync(new byte[] { 0, 0 });

        // When/Then
        await connection.ReceiveAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task ReceiveAsyncHeaderTooBig()
    {
        // Given
        await this.writer.WriteAsync(new byte[] { 10, 40 });

        // When/Then
        await connection.ReceiveAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public async Task ReceiveAsyncBodyClosed()
    {
        // Given
        await this.writer.WriteAsync(new byte[] { 10, 0 });
        this.writer.Complete();

        // When/Then
        await connection.ReceiveAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task ReceiveAsyncInvalidId()
    {
        // Given
        await this.writer.WriteAsync(new byte[] { 18, 0 });
        await this.writer.WriteAsync(Convert.FromHexString("34E45180CE5A3F7946D6A19B80854746"));

        // When/Then
        await connection.ReceiveAsync();
    }

    [TestMethod]
    public async Task ReceiveAsync()
    {
        // Given
        await this.writer.WriteAsync(new byte[] { 18, 0 });
        await this.writer.WriteAsync(Convert.FromHexString("5B452FDDFE94EFCF46D6A19B80854746"));

        // When
        var message = await connection.ReceiveAsync();

        // Then
        Assert.IsInstanceOfType(message, typeof(ClientAuthGameGuard));
    }
}