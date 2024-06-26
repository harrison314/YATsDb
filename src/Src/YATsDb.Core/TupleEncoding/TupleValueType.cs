namespace YATsDb.Core.TupleEncoding;

// Inspiration https://github.com/apple/foundationdb/blob/main/design/tuple.md
internal static class TupleValueType
{
    public const byte TupleNull = 0x00;

    /// <summary>
    /// Byte string prefix, encoding  b'\x01' + value.replace(b'\x00', b'\x00\xFF') + b'\x00'
    /// </summary>
    public const byte TupleByteString = 0x01;

    /// <summary>
    /// Unicode string prefix, Encoding: b'\x02' + value.encode('utf-8').replace(b'\x00', b'\x00\xFF') + b'\x00'
    /// </summary>
    public const byte TupleUnicodeString = 0x02;

    /// <summary>
    /// Nested tuple, terminated [\x00]![\xff]
    /// </summary>
    public const byte TupleNestedTuple = 0x02;

    public const byte TupleFloat = 0x20;
    public const byte TupleDouble = 0x21;
    public const byte TupleLongDouble = 0x23;

    public const byte TupleByte = 0x24;

    public const byte TupleInt32Negative = 0x25;
    public const byte TupleInt32Positive = 0x26;
    public const byte TupleUint32 = 0x27;

    public const byte TupleInt64Negative = 0x28;
    public const byte TupleInt64Positive = 0x29;
    public const byte TupleUint64 = 0x2A;

    public const byte TupleTrue = 0x2B;
    public const byte TupleFalse = 0x2C;

    public const byte TupleGuid = 0x2D;


    public const byte TupleUnixTimestampInMs = 0x31;
}
