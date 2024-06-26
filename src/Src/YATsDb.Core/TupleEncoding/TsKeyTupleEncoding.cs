using System;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YATsDb.Core.TupleEncoding;

internal static class TsKeyTupleEncoding
{
    public static byte[] Encode(uint bucketId, uint measurementId, DateTimeOffset time, uint randomPart)
    {
        return Encode(bucketId, measurementId, time.ToUnixTimeMilliseconds(), randomPart);
    }

    public static byte[] Encode(uint bucketId, uint measurementId, long unixTimestampInMsTime, uint randomPart)
    {
        byte[] keyBytes = GC.AllocateUninitializedArray<byte>(22);
        keyBytes[0] = DataType.KeyTsDataPrefix;
        keyBytes[9] = TupleValueType.TupleUnixTimestampInMs;

        BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(1, 4), bucketId);
        BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(5, 4), measurementId);
        BinaryPrimitives.WriteInt64BigEndian(keyBytes.AsSpan(10, 8), unixTimestampInMsTime);
        BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(18, 4), randomPart);


        return keyBytes;
    }

    public static byte[] Encode(uint bucketId, uint measurementId, long unixTimestampInMsTime, ReadOnlySpan<char> tag, uint randomPart)
    {
        int encodedTagBytes = Encoding.UTF8.GetByteCount(tag);

        byte[] keyBytes = GC.AllocateUninitializedArray<byte>(24 + encodedTagBytes);
        keyBytes[0] = DataType.KeyTsDataIndexPrefix;
        keyBytes[9] = TupleValueType.TupleUnicodeString;

        BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(1, 4), bucketId);
        BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(5, 4), measurementId);
        Encoding.UTF8.TryGetBytes(tag, keyBytes.AsSpan(10), out _);

        keyBytes[encodedTagBytes + 10] = 0x00;
        keyBytes[encodedTagBytes + 11] = TupleValueType.TupleUnixTimestampInMs;

        BinaryPrimitives.WriteInt64BigEndian(keyBytes.AsSpan(encodedTagBytes + 12, 8), unixTimestampInMsTime);
        BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(encodedTagBytes + 20, 4), randomPart);

        return keyBytes;
    }

    public static byte[] EncodePrefix(uint bucketId, uint measurementId, ReadOnlySpan<char> tag)
    {
        if (tag.IsEmpty)
        {
            byte[] keyBytes = GC.AllocateUninitializedArray<byte>(10);
            keyBytes[0] = DataType.KeyTsDataPrefix;
            keyBytes[9] = TupleValueType.TupleUnixTimestampInMs;

            BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(1, 4), bucketId);
            BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(5, 4), measurementId);

            return keyBytes;
        }
        else
        {
            int encodedTagBytes = Encoding.UTF8.GetByteCount(tag);

            byte[] keyBytes = GC.AllocateUninitializedArray<byte>(13 + encodedTagBytes);
            keyBytes[0] = DataType.KeyTsDataIndexPrefix;
            keyBytes[9] = TupleValueType.TupleUnicodeString;

            BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(1, 4), bucketId);
            BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(5, 4), measurementId);
            Encoding.UTF8.TryGetBytes(tag, keyBytes.AsSpan(10), out _);

            keyBytes[encodedTagBytes + 11] = 0x00;
            keyBytes[encodedTagBytes + 12] = TupleValueType.TupleUnixTimestampInMs;

            return keyBytes;
        }
    }

    public static byte[] Encode(uint bucketId, uint measurementId, DateTimeOffset time, ReadOnlySpan<char> tag, uint randomPart)
    {
        return Encode(bucketId, measurementId, time.ToUnixTimeMilliseconds(), tag, randomPart);
    }

    public static bool TryDecodeTime(ReadOnlySpan<byte> key, out long unixTimestampInMsTime)
    {
        if (key.Length < 18 || key[0] != DataType.KeyTsDataPrefix && key[0] != DataType.KeyTsDataIndexPrefix)
        {
            unixTimestampInMsTime = default;
            return false;
        }

        if (key[9] == TupleValueType.TupleUnixTimestampInMs)
        {
            return BinaryPrimitives.TryReadInt64BigEndian(key.Slice(10), out unixTimestampInMsTime);
        }

        if (key[9] == TupleValueType.TupleUnicodeString)
        {
            for (int index = 10; index < key.Length; index++)
            {
                if (key[index] == 0x00)
                {
                    if (index + 9 > key.Length || key[index + 1] != TupleValueType.TupleUnixTimestampInMs)
                    {
                        unixTimestampInMsTime = default;
                        return false;
                    }

                    return BinaryPrimitives.TryReadInt64BigEndian(key.Slice(index + 2), out unixTimestampInMsTime);
                }
            }
        }

        unixTimestampInMsTime = default;
        return false;
    }

    public static int GetTimePosition(ReadOnlySpan<char> tag)
    {
        if (tag.IsEmpty)
        {
            return 10;
        }

        int encodedTagBytes = Encoding.UTF8.GetByteCount(tag);

        return encodedTagBytes + 12;
    }

    public static bool TryDecodeTime(ReadOnlySpan<byte> key, int position, out long unixTimestampInMsTime)
    {
        return BinaryPrimitives.TryReadInt64BigEndian(key.Slice(position), out unixTimestampInMsTime);
    }

    public static string ToString(ReadOnlySpan<byte> key)
    {
        string type = key[0] switch
        {
            DataType.KeyTsDataIndexPrefix => "KeyIndex",
            DataType.KeyTsDataPrefix => "KeyData",
            _ => throw new ArgumentException(nameof(key))
        };

        uint bucketId = BinaryPrimitives.ReadUInt32BigEndian(key.Slice(1, 4));
        uint measurementId = BinaryPrimitives.ReadUInt32BigEndian(key.Slice(5, 4));

        long unixTimestampInMsTime = -1;
        ReadOnlySpan<byte> tag = ReadOnlySpan<byte>.Empty;

        if (key[9] == TupleValueType.TupleUnixTimestampInMs)
        {
            _ = BinaryPrimitives.TryReadInt64BigEndian(key.Slice(10), out unixTimestampInMsTime);
        }

        if (key[9] == TupleValueType.TupleUnicodeString)
        {
            for (int index = 10; index < key.Length; index++)
            {
                if (key[index] == 0x00)
                {
                    tag = key.Slice(10, index - 10);
                    _ = BinaryPrimitives.TryReadInt64BigEndian(key.Slice(index + 2), out unixTimestampInMsTime);
                    break;
                }
            }
        }

        return $"{type}: b={bucketId} m={measurementId} ts={unixTimestampInMsTime}, tag='{Encoding.UTF8.GetString(tag)}'";
    }
}
