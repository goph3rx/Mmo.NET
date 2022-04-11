﻿using System.Numerics;
using System.Security.Cryptography;
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


    [TestMethod]
    public void CalculateChecksum()
    {
        // Given
        var buffer = Convert.FromHexString("0102030405060708");

        // When
        var checksum = CryptHelper.CalculateChecksum(buffer);

        // Then
        Assert.AreEqual(0xC040404, checksum);
    }

    [TestMethod]
    public void DecryptCredentialsSlow()
    {
        // Given
        var d = BigInteger.
            Parse("5028211210128589586589824211007530117579231318758452307666219599177149851309847392553795376833984296340276015783973969792870237836430395460676188261656853110408909556870765308924598039514993678781254980365410044079422036955961003151810138962515343668297202233405097195869073116737041246258368519744337438561").
            ToByteArray(isUnsigned: true, isBigEndian: true);
        var dp = BigInteger.
            Parse("2106908234664237514161794451220044158519300239014035595132746116891070953409700118829772164963720866869225520027120106084167152658823108916151302563592489").
            ToByteArray(isUnsigned: true, isBigEndian: true);
        var dq = BigInteger.
            Parse("1547218264623170192798770435527802553919600917072991162817698767737328527327924624603015678555382993541262142473001497889747837853999540558419921155033").
            ToByteArray(isUnsigned: true, isBigEndian: true);
        var invq = BigInteger.
            Parse("2310407858420994392826869343162164363894415451617348878250406247922465698038111865119601062492866964280209274751271665764913856557440331151631230720569426").
            ToByteArray(isUnsigned: true, isBigEndian: true);
        var p = BigInteger.
            Parse("10782480475963621268594527795533971108611539884762053006263843687544129085866899632027704074279819963455289153991673464972517779462852576061050126199450333").
            ToByteArray(isUnsigned: true, isBigEndian: true);
        var q = BigInteger.
            Parse("10140004340860870492545301803318559597622888530221262183758352414120129969549019612260783852548413524771369703125309916720040404943756788957716637273739773").
            ToByteArray(isUnsigned: true, isBigEndian: true);
        var n = BigInteger.
            Parse("109334398831518704623867720410351858432577996993189346014439626367376532782114621289249531390699677780110374667031951578737669799962222570440058178534905522679906417895705877281498935851765656707649087726582568276619818331804780785381812661807889694188761532156756869778893120843385293190663856749007880194409").
            ToByteArray(isUnsigned: true, isBigEndian: true);
        var e = new BigInteger(65537).
            ToByteArray(isUnsigned: true, isBigEndian: true);
        var key = RSA.Create(new RSAParameters {
            Exponent = e,
            Modulus = n,
            D = d,
            P = p,
            DP = dp,
            Q = q,
            DQ = dq,
            InverseQ = invq,
        });
        var credentials = Convert.FromHexString("5FD6C209035E918F2CA6888F1BCE611C1BB26E01855EC30AEBBA4D5E72B3D1BA86434F9EBBE68AFD5A530A431CDF09DD8660F5D0BD556C9790DA4E279B21B6261C9142AA570860ECD4B37C0547774CF37C647037569AB30F2D7D87DCD5778E2BC7AB88B3E41A04A4EEEA72CE228851508E278F4F0BC2E8099554930F886ED323");

        // When
        var decrypted = CryptHelper.DecryptCredentials(credentials, key);

        // Then
        Assert.AreEqual(
            "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000024000068656C6C6F000000000000000000776F726C64000000000000000000000000000000",
            Convert.ToHexString(decrypted)
        );
    }
}