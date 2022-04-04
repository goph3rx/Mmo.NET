using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mmo.AuthServer.Crypt;

namespace Mmo.Tests.Crypt;

[TestClass]
public class AuthCryptHelperTest
{
    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void ScrambleModulusInvalidSize()
    {
        // Given
        var modulus = Array.Empty<byte>();

        // When/Then
        CryptHelper.ScrambleModulus(modulus);
    }

    [TestMethod]
    public void ScrambleModulus()
    {
        // Given
        var modulus = Convert.FromHexString("9A277669023723947D0EBDCCEF967A24C715018DF6CE66414FCCD0F5BAB54124B8CAAC6D7F52F8BBBAB7DE926B4F0AC4CC84793196E44928774A57737D0E4EE02962952257506E898846E353FA5FEE31409A1D32124FB8DF53D969DD7AA222866FA85E106F8A07E333D8DED4B10A8300B32D5F47CC5EAB14033FA2BC0950B5C9");

        // When
        CryptHelper.ScrambleModulus(modulus);

        // Then
        Assert.AreEqual(
            "768CA46255674D1DF5485E9F1556E7B0928F1CBFE481DE9E1C15B928C01763A2D762F27D10D8FF58896F0046DA4589C47FA926765ABAE23C7475F5CF745EFB295FEE3140023723947D0EBDCCEFCCC0C6FB15018DF6CE66414FCCD0F5BAB54124B8CAAC6D7F52F8BBBAB7DE926B4F0AC4CC84793196E44928774A57737D0E4EE0",
            Convert.ToHexString(modulus)
        );
    }

    [TestMethod]
    public void ScrambleInit()
    {
        // Given
        var buffer = Convert.FromHexString("010203040506070800000000");
        var key = 0xDEADBEEF;

        // When
        CryptHelper.ScrambleInit(buffer, key);

        // Then
        Assert.AreEqual("01020304F1C2B3EEF4C4B4E6", Convert.ToHexString(buffer));
    }
}