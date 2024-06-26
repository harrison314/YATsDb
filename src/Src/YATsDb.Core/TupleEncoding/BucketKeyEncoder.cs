using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YATsDb.Core.TupleEncoding;

internal static class BucketKeyEncoder
{
    public static byte[] GetPrefix()
    {
        return new byte[]
        {
            DataType.KeyBucketPrefix,
            //0,
            //0,
            //0
        };
    }

    public static byte[] Encode(ReadOnlySpan<char> bucketName)
    {
        int encodedTagBytes = Encoding.UTF8.GetByteCount(bucketName);

        byte[] keyBytes = GC.AllocateUninitializedArray<byte>(4 + encodedTagBytes);
        keyBytes[0] = DataType.KeyBucketPrefix;
        keyBytes[1] = 0;

        keyBytes[2] = TupleValueType.TupleUnicodeString;
        keyBytes[encodedTagBytes + 3] = 0x00;

        Encoding.UTF8.TryGetBytes(bucketName, keyBytes.AsSpan(3), out _);

        return keyBytes;
    }

    public static string Decode(ReadOnlySpan<byte> value)
    {
        if (value[0] != DataType.KeyBucketPrefix) throw new ArgumentException();

        return Encoding.UTF8.GetString(value[3..^1]);
    }

    public static bool TryDecode(ReadOnlySpan<byte> value, out string name)
    {
        if (value[0] == DataType.KeyBucketPrefix)
        {
            name = Encoding.UTF8.GetString(value[3..^1]);
            return true;
        }

        name = string.Empty;
        return false;
    }
}
