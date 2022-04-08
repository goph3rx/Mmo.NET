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

    [TestMethod]
    public void RequestAuthLogin()
    {
        // Given
        var buffer = Convert.FromHexString("007E6C5A765631F7102C30505DF2AE630B725E25F9F95AC1A66330C2074B229598D75E48D30F8848D8C30CBBC9D2A6E36AB502CC028FCCBA58A6CBFB9D9164EA13129D03EEFFF00E383D38694E2B0E7225BB576F6E4D37097C6299B7BB06C47E29B7F2A48AD11781EB93C039E3F9F9F7D63D91BBF5B8AB7DD7F038A83DC22CB3000B");
        var reader = new PacketReader(buffer);

        // When
        var result = ClientMessages.ReadFrom(reader);

        // Then
        var message = (ClientRequestAuthLogin)result;
        Assert.AreEqual(
            "7E6C5A765631F7102C30505DF2AE630B725E25F9F95AC1A66330C2074B229598D75E48D30F8848D8C30CBBC9D2A6E36AB502CC028FCCBA58A6CBFB9D9164EA13129D03EEFFF00E383D38694E2B0E7225BB576F6E4D37097C6299B7BB06C47E29B7F2A48AD11781EB93C039E3F9F9F7D63D91BBF5B8AB7DD7F038A83DC22CB300",
            Convert.ToHexString(message.Credentials)
        );
    }
}
