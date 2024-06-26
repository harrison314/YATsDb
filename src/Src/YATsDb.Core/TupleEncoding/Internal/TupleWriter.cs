using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.TupleEncoding.Internal;

internal ref struct TupleWriter
{
    int size = 1;
    byte[]? buffer;
    byte type;
    int pointer = 0;

    public TupleWriter(byte type)
    {
        this.buffer = null;
        this.type = type;
    }

    public void ReserveType<T>(in T value)
    {
#pragma warning disable CS8605 // Unboxing a possibly null value.

        if (typeof(T) == typeof(uint))
        {
            this.size += 5;
        }
        else if (typeof(T) == typeof(long))
        {
            this.size += 9;
        }
        else if (typeof(T) == typeof(string))
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
            this.size += Encoding.UTF8.GetByteCount((string)(object)value) + 2;
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        else if (typeof(T) == typeof(ReadOnlyMemory<char>))
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            this.size += Encoding.UTF8.GetByteCount(((ReadOnlyMemory<char>)(object)value).Span) + 2;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        }
        else if (typeof(T) == typeof(Memory<char>))
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            this.size += Encoding.UTF8.GetByteCount(((Memory<char>)(object)value).Span) + 2;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        else if (typeof(T) == typeof(byte))
        {
            this.size += 2;
        }
        else if (typeof(T) == typeof(bool))
        {
            this.size += 1;
        }
        else if (typeof(T) == typeof(int))
        {
            this.size += 5;
        }
        else if (typeof(T) == typeof(Guid))
        {
            this.size += 17;
        }
        else
        {
            throw new InvalidProgramException($"Unsupported type {typeof(T).Name} for tuple");
        }
#pragma warning restore CS8605 // Unboxing a possibly null value.
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AllocateBuffer()
    {
        this.buffer = GC.AllocateUninitializedArray<byte>(this.size);
        this.buffer[0] = this.type;
        this.pointer = 1;
    }

    internal void WriteBool(bool value)
    {
        System.Diagnostics.Debug.Assert(this.buffer != null);

        this.buffer[this.pointer++] = value ? TupleValueType.TupleTrue : TupleValueType.TupleFalse;
    }

    internal void WriteByte(byte value)
    {
        System.Diagnostics.Debug.Assert(this.buffer != null);

        this.buffer[this.pointer++] = TupleValueType.TupleByte;
        this.buffer[this.pointer++] = value;
    }
    internal void WriteUint32(uint value)
    {
        System.Diagnostics.Debug.Assert(this.buffer != null);

        this.buffer[this.pointer++] = TupleValueType.TupleUint32;
        BinaryPrimitives.WriteUInt32BigEndian(this.buffer.AsSpan(this.pointer), value);
        this.pointer += sizeof(uint);
    }

    internal void WriteInt64(long value)
    {
        System.Diagnostics.Debug.Assert(this.buffer != null);

        if (value < 0)
        {
            value = ~-value;
            this.buffer[this.pointer++] = TupleValueType.TupleInt64Negative;
            BinaryPrimitives.WriteInt64BigEndian(this.buffer.AsSpan(this.pointer), value);
        }
        else
        {
            this.buffer[this.pointer++] = TupleValueType.TupleInt64Positive;
            BinaryPrimitives.WriteInt64BigEndian(this.buffer.AsSpan(this.pointer), value);
        }

        this.pointer += sizeof(long);
    }

    internal void WriteInt32(int value)
    {
        System.Diagnostics.Debug.Assert(this.buffer != null);

        if (value < 0)
        {
            value = ~-value;
            this.buffer[this.pointer++] = TupleValueType.TupleInt32Negative;
            BinaryPrimitives.WriteInt32BigEndian(this.buffer.AsSpan(this.pointer), value);
        }
        else
        {
            this.buffer[this.pointer++] = TupleValueType.TupleInt32Positive;
            BinaryPrimitives.WriteInt32BigEndian(this.buffer.AsSpan(this.pointer), value);
        }

        this.pointer += sizeof(int);
    }

    internal void WriteString(ReadOnlySpan<char> value)
    {
        System.Diagnostics.Debug.Assert(this.buffer != null);
        this.buffer[this.pointer++] = TupleValueType.TupleUnicodeString;
        Encoding.UTF8.TryGetBytes(value, this.buffer.AsSpan(this.pointer), out int writes);
        this.pointer += writes;
        this.buffer[this.pointer++] = 0;
    }

    internal void WriteGuid(Guid value)
    {
        System.Diagnostics.Debug.Assert(this.buffer != null);
        this.buffer[this.pointer++] = TupleValueType.TupleGuid;
        value.TryWriteBytes(this.buffer.AsSpan(this.pointer));

        this.pointer += 16;
    }

    public void Write<T>(in T value)
    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        if (typeof(T) == typeof(uint))
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            this.WriteUint32((uint)(object)value);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }
        else if (typeof(T) == typeof(long))
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            this.WriteInt64((long)(object)value);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }
        else if (typeof(T) == typeof(string))
        {
            this.WriteString((string)(object)value);
        }
        else if (typeof(T) == typeof(ReadOnlyMemory<char>))
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            this.WriteString(((ReadOnlyMemory<char>)(object)value).Span);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }
        else if (typeof(T) == typeof(Memory<char>))
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            this.WriteString(((ReadOnlyMemory<char>)(object)value).Span);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }
        else if (typeof(T) == typeof(byte))
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            this.WriteByte((byte)(object)value);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }
        else if (typeof(T) == typeof(bool))
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            this.WriteBool((bool)(object)value);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }
        else if (typeof(T) == typeof(int))
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            this.WriteInt32((int)(object)value);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }
        else if (typeof(T) == typeof(Guid))
        {
#pragma warning disable CS8605 // Unboxing a possibly null value.
            this.WriteGuid((Guid)(object)value);
#pragma warning restore CS8605 // Unboxing a possibly null value.
        }
        else
        {
            throw new InvalidProgramException($"Unsupported type {typeof(T).Name} for tuple");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ToArray()
    {
        System.Diagnostics.Debug.Assert(this.buffer != null);

        return this.buffer;
    }
}
