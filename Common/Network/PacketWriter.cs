using System.Buffers.Binary;

namespace Mmo.Common.Network;

/// <summary>
/// Provides primitives for serializing packets.
/// </summary>
public ref struct PacketWriter
{
    private readonly Span<byte> memory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PacketWriter"/> struct.
    /// </summary>
    /// <param name="memory">Underlying memory.</param>
    public PacketWriter(Span<byte> memory)
    {
        this.memory = memory;

        // Reset the memory
        this.memory.Fill(0);
    }

    /// <summary>
    /// Gets the length of the written data.
    /// </summary>
    public int Length { get; private set; } = 0;

    /// <summary>
    /// Skip a given number of bytes.
    /// </summary>
    /// <param name="length">How many bytes to skip.</param>
    /// <exception cref="Exception">If skipping causes an overflow.</exception>
    public void Skip(int length)
    {
        if (this.Length + length > this.memory.Length)
        {
            throw new ArgumentException("Memory overflow", nameof(length));
        }

        this.Length += length;
    }

    /// <summary>
    /// Write a C value (1 byte).
    /// </summary>
    /// <param name="value">Value to write.</param>
    public void WriteC(byte value)
    {
        this.memory[this.Length] = value;
        this.Length++;
    }

    /// <summary>
    /// Write a H value (2 bytes).
    /// </summary>
    /// <param name="value">Value to write.</param>
    public void WriteH(short value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(this.memory[this.Length..], value);
        this.Length += 2;
    }

    /// <summary>
    /// Write a D value (4 bytes).
    /// </summary>
    /// <param name="value">Value to write.</param>
    public void WriteD(int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(this.memory[this.Length..], value);
        this.Length += 4;
    }

    /// <summary>
    /// Write a B value.
    /// </summary>
    /// <param name="value">Value to write.</param>
    public void WriteB(ReadOnlySpan<byte> value)
    {
        value.CopyTo(this.memory[this.Length..]);
        this.Length += value.Length;
    }

    /// <summary>
    /// Get the written data as a span.
    /// </summary>
    /// <returns>Written data as a span.</returns>
    public readonly Span<byte> AsSpan()
    {
        return this.memory[..this.Length];
    }
}