using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mmo.AuthServer.Account;
using Moq;

namespace Mmo.Tests.Account;

[TestClass]
public class AccountServiceTest
{
    private Mock<IAccountRepository> repository;
    private AccountService service;

    public AccountServiceTest()
    {
        this.repository = new Mock<IAccountRepository>();
        this.service = new AccountService(this.repository.Object);
    }

    [TestMethod]
    public void Create()
    {
        // Given
        this.repository.
            Setup(x => x.Create("hello", It.Is<byte[]>(s => s.Length == 16), It.Is<byte[]>(p => p.Length == 32))).
            Returns(Task.CompletedTask).
            Verifiable();

        // When
        this.service.Create("hello", "world");

        // Then
        this.repository.Verify();
    }
}
