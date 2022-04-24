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
    public async Task CreateAsync()
    {
        // Given
        this.repository.
            Setup(x => x.CreateAsync("hello", It.Is<byte[]>(s => s.Length == 16), It.Is<byte[]>(p => p.Length == 32))).
            Returns(Task.CompletedTask).
            Verifiable();

        // When
        await this.service.CreateAsync("hello", "world");

        // Then
        this.repository.Verify();
    }

    [TestMethod]
    public async Task FindAsyncNotFound()
    {
        // Given
        this.repository.
            Setup(x => x.FetchAsync("hello")).
            ReturnsAsync((AccountRecord?)null).
            Verifiable();

        // When
        var account = await this.service.FindAsync("hello", "world");

        // Then
        Assert.IsNull(account);
        this.repository.Verify();
    }

    [TestMethod]
    public async Task FindAsyncInvalidPassword()
    {
        // Given
        this.repository.
            Setup(x => x.FetchAsync("hello")).
            ReturnsAsync(new AccountRecord(
                Username: "hello",
                Salt: Convert.FromHexString("BBF96DF3C7C50C46C08BF23418F7364B"),
                Password: Convert.FromHexString("CA132F6A4A1636DD3A55504B9799FCDB997C36408DBA84F7581D99471EDFBAEC"),
                LastWorld: 0,
                IsBanned: false
            )).
            Verifiable();

        // When
        var account = await this.service.FindAsync("hello", "world2");

        // Then
        Assert.IsNull(account);
        this.repository.Verify();
    }

    [TestMethod]
    public async Task FindAsync()
    {
        // Given
        this.repository.
            Setup(x => x.FetchAsync("hello")).
            ReturnsAsync(new AccountRecord(
                Username: "hello",
                Salt: Convert.FromHexString("BBF96DF3C7C50C46C08BF23418F7364B"),
                Password: Convert.FromHexString("CA132F6A4A1636DD3A55504B9799FCDB997C36408DBA84F7581D99471EDFBAEC"),
                LastWorld: 1,
                IsBanned: true
            )).
            Verifiable();

        // When
        var account = await this.service.FindAsync("hello", "world");

        // Then
        Assert.IsNotNull(account);
        Assert.AreEqual("hello", account.Username);
        Assert.AreEqual(1, account.LastWorld);
        Assert.AreEqual(true, account.IsBanned);
        this.repository.Verify();
    }
}
