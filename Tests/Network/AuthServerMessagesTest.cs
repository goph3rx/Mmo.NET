using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mmo.AuthServer.Network;
using Mmo.Common.Network;

namespace Mmo.Tests.Network;

[TestClass]
public class AuthServerMessagesTest
{
    private byte[] memory = new byte[1024];

    [TestMethod]
    public void Init()
    {
        // Given
        var message = new ServerInit(
            SessionId: unchecked((int)0xDEADBEEF),
            Modulus: Convert.FromHexString("9A277669023723947D0EBDCCEF967A24C715018DF6CE66414FCCD0F5BAB54124B8CAAC6D7F52F8BBBAB7DE926B4F0AC4CC84793196E44928774A57737D0E4EE02962952257506E898846E353FA5FEE31409A1D32124FB8DF53D969DD7AA222866FA85E106F8A07E333D8DED4B10A8300B32D5F47CC5EAB14033FA2BC0950B5C9"),
            CryptKey: Convert.FromHexString("060708090A")
        );
        var writer = new PacketWriter(this.memory);

        // When
        message.WriteTo(ref writer);

        // Then
        Assert.AreEqual(
            "00EFBEADDE21C60000768CA46255674D1DF5485E9F1556E7B0928F1CBFE481DE9E1C15B928C01763A2D762F27D10D8FF58896F0046DA4589C47FA926765ABAE23C7475F5CF745EFB295FEE3140023723947D0EBDCCEFCCC0C6FB15018DF6CE66414FCCD0F5BAB54124B8CAAC6D7F52F8BBBAB7DE926B4F0AC4CC84793196E44928774A57737D0E4EE000000000000000000000000000000000060708090A",
            Convert.ToHexString(writer.AsSpan())
        );
    }

}

