using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mmo.AuthServer;
using Mmo.AuthServer.Network;
using Moq;

namespace Mmo.Tests.Network;

[TestClass]
public class AuthClientTest
{
    private Mock<ILogger<Client>> logger;
    private Mock<IConnection> connection;
    private Client client;

    public AuthClientTest()
    {
        this.logger = new Mock<ILogger<Client>>();
        this.connection = new Mock<IConnection>();
        this.client = new Client(this.connection.Object, this.logger.Object);
    }

    [TestMethod]
    public async Task InitAsync()
    {
        // Given
        this.connection.
            Setup(x => x.SendAsync(It.Is<ServerInit>(m => m.Modulus.Length == 128 && m.CryptKey.Length == 16))).
            Returns(Task.CompletedTask).
            Verifiable();

        // When
        await this.client.InitAsync();

        // Then
        this.connection.Verify();
    }

    [TestMethod]
    public async Task HandleAuthGameGuardAsync()
    {
        // Given
        this.connection.
            SetupSequence(x => x.ReceiveAsync()).
            ReturnsAsync(new ClientAuthGameGuard()).
            Throws(new ConnectionClosedException());
        this.connection.
            Setup(x => x.SendAsync(It.Is<ServerGgAuth>(m => m.Result == GgAuthResult.Skip))).
            Returns(Task.CompletedTask).
            Verifiable();

        // When
        await this.client.RunAsync();

        // Then
        this.connection.Verify();
    }
}
