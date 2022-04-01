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
        public byte[] CryptKey => Array.Empty<byte>();

        public void WriteTo(ref PacketWriter writer)
        {
            for (byte i = 1; i <= 16; i++)
            {
                writer.WriteC(i);
            }
        }
    }

    class FakePipeWriter : PipeWriter
    {
        public byte[] Buffer = new byte[Connection.BufferSize];
        public int Advanced;
        public bool Flushed;

        public override void Advance(int bytes)
        {
            Advanced = bytes;
        }

        public override void CancelPendingFlush()
        {
            throw new NotImplementedException();
        }

        public override void Complete(Exception? exception = null)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
        {
            Flushed = true;
            return new ValueTask<FlushResult>();
        }

        public override Memory<byte> GetMemory(int sizeHint = 0)
        {
            throw new NotImplementedException();
        }

        public override Span<byte> GetSpan(int sizeHint = 0)
        {
            return Buffer;
        }
    }

    private FakePipeWriter output;
    private EndPoint endPoint;
    private Connection connection;

    public AuthConnectionTest()
    {
        this.output = new FakePipeWriter();
        var pipe = new Mock<IDuplexPipe>();
        pipe.Setup(x => x.Output).Returns(this.output);
        this.endPoint = new IPEndPoint(0x7F000001, 8123);
        this.connection = new Connection(pipe.Object, this.endPoint, 0xD5906CBC);
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
        Assert.AreEqual(26, this.output.Advanced);
        Assert.IsTrue(this.output.Flushed);
        Assert.AreEqual(
            "1A009BB6D905778ECB0ECF113FD58BFB83A5E8D00DA1A97F42CD",
            Convert.ToHexString(this.output.Buffer[..this.output.Advanced])
        );
    }
}