using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mmo.Common.Network;

namespace Mmo.Tests.Network;

[TestClass]
public class PacketReaderTest
{
    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void ReadCOverflow()
    {
        // Given
        var memory = Array.Empty<byte>();
        var writer = new PacketReader(memory);

        // When/Then
        writer.ReadC();
    }

    [TestMethod]
    public void ReadC()
    {
        // Given
        var memory = Convert.FromHexString("7B");
        var writer = new PacketReader(memory);

        // When
        var value = writer.ReadC();

        // Then
        Assert.AreEqual(0x7B, value);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ReadHOverflow()
    {
        // Given
        var memory = Array.Empty<byte>();
        var writer = new PacketReader(memory);

        // When/Then
        writer.ReadH();
    }

    [TestMethod]
    public void ReadH()
    {
        // Given
        var memory = Convert.FromHexString("7B10");
        var writer = new PacketReader(memory);

        // When
        var value = writer.ReadH();

        // Then
        Assert.AreEqual(0x107B, value);
    }
}
