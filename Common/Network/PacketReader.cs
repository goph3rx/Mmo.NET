﻿using System.Buffers;
using System.Buffers.Binary;

namespace Mmo.Common.Network;

/// <summary>
/// Provides primitives for deserializing packets.
/// </summary>
public ref struct PacketReader
{
    private ReadOnlySpan<byte> memory;
    private int offset = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketReader"/> struct.
    /// </summary>
    /// <param name="memory">Memory to read.</param>
    public PacketReader(ReadOnlySpan<byte> memory)
    {
        this.memory = memory;
    }

    /// <summary>
    /// Read a C value (1 byte).
    /// </summary>
    /// <returns>Read value.</returns>
    public byte ReadC()
    {
        var value = this.memory[this.offset];
        this.offset++;
        return value;
    }

    /// <summary>
    /// Read a H value (2 bytes).
    /// </summary>
    /// <returns>Read value.</returns>
    /// <exception cref="InvalidOperationException">When buffer end was reached.</exception>
    public short ReadH()
    {
        var value = BinaryPrimitives.ReadInt16LittleEndian(this.memory[this.offset..]);
        this.offset += 2;
        return value;
    }
}
