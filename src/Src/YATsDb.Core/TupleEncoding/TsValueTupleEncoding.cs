using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.TupleEncoding;

internal static class TsValueTupleEncoding
{
    public const int MaxTsValues = 200;

    public static byte[] Encode(ReadOnlySpan<double> values, ReadOnlySpan<char> tag)
    {
#if DEBUG
        if (values.Length == 0) throw new ArgumentException("Empty values does not supported.");
        if (values.Length > MaxTsValues) throw new ArgumentException($"More than {MaxTsValues} value.");
#endif

        int tagLen = tag.Length == 0 ? 0 : Encoding.UTF8.GetByteCount(tag);
        byte[] value = GC.AllocateUninitializedArray<byte>(1 + sizeof(double) * values.Length + (tagLen == 0 ? 1 : tagLen + 2));

        value[0] = (byte)values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteDoubleBigEndian(value.AsSpan(1 + sizeof(double) * i), values[i]);
        }

        if (tagLen == 0)
        {
            value[1 + sizeof(double) * values.Length] = TupleValueType.TupleNull;
        }
        else
        {
            value[1 + sizeof(double) * values.Length] = TupleValueType.TupleUnicodeString;
            Encoding.UTF8.GetBytes(tag, value.AsSpan(2 + sizeof(double) * values.Length));
            value[^1] = 0x00;
        }

        return value;
    }

    public static void DecodeValues(ReadOnlySpan<byte> value, ReadOnlySpan<int> indexes, Span<double> values)
    {
#if DEBUG
        if (value[0] == DataType.RemovedValue) throw new ArgumentException("Value is removed.");
        if (indexes.Length > values.Length) throw new IndexOutOfRangeException("indexes is shorter than values");
#endif

        int valueCount = value[0];
        int index;

        for (int i = 0; i < indexes.Length; i++)
        {
            index = indexes[i];
            if (index < valueCount)
            {
                values[i] = BinaryPrimitives.ReadDoubleBigEndian(value.Slice(1 + index * sizeof(double)));
            }
            else
            {
                values[i] = double.NaN;
            }
        }
    }

    public static double[] DecodeAllValues(ReadOnlySpan<byte> value)
    {
#if DEBUG
        if (value[0] == DataType.RemovedValue) throw new ArgumentException("Value is removed.");
#endif

        int valueCount = value[0];
        double[] values = new double[valueCount];

        for (int i = 0; i < valueCount; i++)
        {
            values[i] = BinaryPrimitives.ReadDoubleBigEndian(value.Slice(1 + i * sizeof(double)));
        }

        return values;
    }

    public static ReadOnlyMemory<byte> DecodeTagData(Memory<byte> value)
    {
#if DEBUG
        if (value.Span[0] == DataType.RemovedValue) throw new ArgumentException("Value is removed.");
#endif

        int valueCount = value.Span[0];
        byte tagFlag = value.Span[1 + sizeof(double) * valueCount];

        if (tagFlag == TupleValueType.TupleNull)
        {
            return Memory<byte>.Empty;
        }

        if (tagFlag == TupleValueType.TupleUnicodeString)
        {
            int startIndex = 2 + sizeof(double) * valueCount;
            int strLen = value.Length - startIndex - 1;
            return value.Slice(startIndex, strLen);
        }

        throw new InvalidDataException("Invalid tag flag");
    }
}
