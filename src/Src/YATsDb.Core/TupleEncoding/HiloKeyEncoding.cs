using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.TupleEncoding;

internal static class HiloKeyEncoding
{
    public static byte[] Encode(HiloType type)
    {
        byte[] keyBytes = GC.AllocateUninitializedArray<byte>(4);
        keyBytes[0] = DataType.KeyHiloPrefix;
        keyBytes[1] = (byte)type;
        keyBytes[2] = 0;
        keyBytes[3] = 0;

        return keyBytes;
    }

    public static byte[] Encode(HiloType type, uint prefixId1)
    {
        byte[] keyBytes = GC.AllocateUninitializedArray<byte>(6);
        keyBytes[0] = DataType.KeyHiloPrefix;
        keyBytes[1] = (byte)type;
        BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(2, 4), prefixId1);

        return keyBytes;
    }

    public static byte[] Encode(HiloType type, uint prefixId1, uint prefixId2, ReadOnlySpan<char> name)
    {
        int encodedTagBytes = Encoding.UTF8.GetByteCount(name);

        byte[] keyBytes = GC.AllocateUninitializedArray<byte>(12 + encodedTagBytes);
        keyBytes[0] = DataType.KeyHiloPrefix;
        keyBytes[1] = (byte)type;

        keyBytes[10] = TupleValueType.TupleUnicodeString;
        keyBytes[encodedTagBytes + 11] = 0x00;

        BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(2, 4), prefixId1);
        BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(6, 4), prefixId2);

        Encoding.UTF8.TryGetBytes(name, keyBytes.AsSpan(11), out _);

        return keyBytes;
    }

    public static byte[] Encode(HiloType type, uint prefixId1, uint prefixId2, uint prefixId3, ReadOnlySpan<char> name)
    {
        int encodedTagBytes = Encoding.UTF8.GetByteCount(name);

        byte[] keyBytes = GC.AllocateUninitializedArray<byte>(16 + encodedTagBytes);
        keyBytes[0] = DataType.KeyHiloPrefix;
        keyBytes[1] = (byte)type;

        keyBytes[14] = TupleValueType.TupleUnicodeString;
        keyBytes[encodedTagBytes + 15] = 0x00;

        BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(2, 4), prefixId1);
        BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(6, 4), prefixId2);
        BinaryPrimitives.WriteUInt32BigEndian(keyBytes.AsSpan(10, 4), prefixId2);

        Encoding.UTF8.TryGetBytes(name, keyBytes.AsSpan(15), out _);

        return keyBytes;
    }
}
