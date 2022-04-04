using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mmo.AuthServer.Network;
using Mmo.Common.Network;

namespace Mmo.Tests.Network;

[TestClass]
public class AuthClientMessagesTest
{
    [TestMethod]
    public void AuthGameGuard()
    {
        // Given
        var buffer = Convert.FromHexString("0725C7892400000000000000000000000000000000000000");
        var reader = new PacketReader(buffer);

        // When
        var result = ClientMessages.ReadFrom(reader);

        // Then
        if (result is ClientAuthGameGuard)
        {
            return;
        }

        Assert.Fail();
    }
}
