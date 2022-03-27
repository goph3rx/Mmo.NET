using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mmo.AuthServer.Crypt;

namespace Mmo.Tests.Crypt;

[TestClass]
public class AuthCipherTest
{
    [TestMethod]
    public void Encrypt()
    {
        // Given
        var buffer = Convert.FromHexString("0102030405060708090A0B0C0D0E0F10");
        var cipher = new Cipher();

        // When
        cipher.Encrypt(buffer);

        // Then
        Assert.AreEqual("569A83DF25C8816A99368A84726F4FD7", Convert.ToHexString(buffer));
    }
}

