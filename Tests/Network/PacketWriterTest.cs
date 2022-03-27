using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mmo.Common.Network;

namespace Mmo.Tests.Network;

[TestClass]
public class PacketWriterTest
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SkipOverflow()
    {
        // Given
        var writer = new PacketWriter();

        // When/Then
        writer.Skip(1);
    }

    [TestMethod]
    public void Skip()
    {
        // Given
        var memory = new byte[16];
        var writer = new PacketWriter(memory);

        // When
        writer.Skip(8);

        // Then
        Assert.AreEqual("0000000000000000", Convert.ToHexString(writer.AsSpan()));
    }

    [TestMethod]
    [ExpectedException(typeof(IndexOutOfRangeException))]
    public void WriteCOverflow()
    {
        // Given
        var memory = Array.Empty<byte>();
        var writer = new PacketWriter(memory);

        // When/Then
        writer.WriteC(0x7B);
    }

    [TestMethod]
    public void WriteC()
    {
        // Given
        var memory = new byte[1];
        var writer = new PacketWriter(memory);

        // When
        writer.WriteC(0x7B);

        // Then
        Assert.AreEqual("7B", Convert.ToHexString(writer.AsSpan()));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void WriteDOverflow()
    {
        // Given
        var memory = Array.Empty<byte>();
        var writer = new PacketWriter(memory);

        // When/Then
        writer.WriteD(0x105C6A7B);
    }

    [TestMethod]
    public void WriteD()
    {
        // Given
        var memory = new byte[16];
        var writer = new PacketWriter(memory);

        // When
        writer.WriteD(0x105C6A7B);

        // Then
        Assert.AreEqual("7B6A5C10", Convert.ToHexString(writer.AsSpan()));
    }


    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void WriteBOverflow()
    {
        // Given
        var memory = Array.Empty<byte>();
        var writer = new PacketWriter(memory);

        // When/Then
        writer.WriteB(new byte[] { 0 });
    }

    [TestMethod]
    public void WriteB()
    {
        // Given
        var memory = new byte[16];
        var writer = new PacketWriter(memory);

        // When
        writer.WriteB(new byte[] { 1, 2, 3 });

        // Then
        Assert.AreEqual("010203", Convert.ToHexString(writer.AsSpan()));
    }
}
