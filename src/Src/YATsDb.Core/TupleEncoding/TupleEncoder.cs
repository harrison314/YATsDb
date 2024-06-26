using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YATsDb.Core.TupleEncoding.Internal;

namespace YATsDb.Core.TupleEncoding;

internal class TupleEncoder
{
    public static byte[] Create<T1>(byte type, T1 value1)
    {
        TupleWriter tw = new TupleWriter(type);
        tw.ReserveType(value1);
        tw.AllocateBuffer();
        tw.Write(value1);

        return tw.ToArray();
    }

    public static bool TryDeconstruct<T1>(ReadOnlySpan<byte> key, byte type, out T1 value1)
    {
        value1 = default!;
        int position = 1;

        return key[0] == type
            && TupleReader.TryRead(key, ref position, out value1);
    }

    public static byte[] Create<T1, T2>(byte type, T1 value1, T2 value2)
    {
        TupleWriter tw = new TupleWriter(type);
        tw.ReserveType(value1);
        tw.ReserveType(value2);
        tw.AllocateBuffer();
        tw.Write(value1);
        tw.Write(value2);

        return tw.ToArray();
    }

    public static bool TryDeconstruct<T1, T2>(ReadOnlySpan<byte> key, byte type, out T1 value1, out T2 value2)
    {
        value1 = default!;
        value2 = default!;
        int position = 1;

        return key[0] == type
            && TupleReader.TryRead(key, ref position, out value1)
            && TupleReader.TryRead(key, ref position, out value2);
    }

    public static byte[] Create<T1, T2, T3>(byte type, T1 value1, T2 value2, T3 value3)
    {
        TupleWriter tw = new TupleWriter(type);
        tw.ReserveType(value1);
        tw.ReserveType(value2);
        tw.ReserveType(value3);
        tw.AllocateBuffer();
        tw.Write(value1);
        tw.Write(value2);
        tw.Write(value3);

        return tw.ToArray();
    }

    public static bool TryDeconstruct<T1, T2, T3>(ReadOnlySpan<byte> key, byte type, out T1 value1, out T2 value2, out T3 value3)
    {
        value1 = default!;
        value2 = default!;
        value3 = default!;
        int position = 1;

        return key[0] == type
            && TupleReader.TryRead(key, ref position, out value1)
            && TupleReader.TryRead(key, ref position, out value2)
            && TupleReader.TryRead(key, ref position, out value3);
    }

    public static byte[] Create<T1, T2, T3, T4>(byte type, T1 value1, T2 value2, T3 value3, T4 value4)
    {
        TupleWriter tw = new TupleWriter(type);
        tw.ReserveType(value1);
        tw.ReserveType(value2);
        tw.ReserveType(value3);
        tw.ReserveType(value4);
        tw.AllocateBuffer();
        tw.Write(value1);
        tw.Write(value2);
        tw.Write(value3);
        tw.Write(value4);

        return tw.ToArray();
    }

    public static bool TryDeconstruct<T1, T2, T3, T4>(ReadOnlySpan<byte> key, byte type, out T1 value1, out T2 value2, out T3 value3, out T4 value4)
    {
        value1 = default!;
        value2 = default!;
        value3 = default!;
        value4 = default!;
        int position = 1;

        return key[0] == type
            && TupleReader.TryRead(key, ref position, out value1)
            && TupleReader.TryRead(key, ref position, out value2)
            && TupleReader.TryRead(key, ref position, out value3)
            && TupleReader.TryRead(key, ref position, out value4);
    }

    public static byte[] Create<T1, T2, T3, T4, T5>(byte type, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
    {
        TupleWriter tw = new TupleWriter(type);
        tw.ReserveType(value1);
        tw.ReserveType(value2);
        tw.ReserveType(value3);
        tw.ReserveType(value4);
        tw.ReserveType(value5);
        tw.AllocateBuffer();
        tw.Write(value1);
        tw.Write(value2);
        tw.Write(value3);
        tw.Write(value4);
        tw.Write(value5);

        return tw.ToArray();
    }

    public static bool TryDeconstruct<T1, T2, T3, T4, T5>(ReadOnlySpan<byte> key, byte type, out T1 value1, out T2 value2, out T3 value3, out T4 value4, out T5 value5)
    {
        value1 = default!;
        value2 = default!;
        value3 = default!;
        value4 = default!;
        value5 = default!;
        int position = 1;

        return key[0] == type
            && TupleReader.TryRead(key, ref position, out value1)
            && TupleReader.TryRead(key, ref position, out value2)
            && TupleReader.TryRead(key, ref position, out value3)
            && TupleReader.TryRead(key, ref position, out value4)
            && TupleReader.TryRead(key, ref position, out value5);
    }

    public static byte[] Create<T1, T2, T3, T4, T5, T6>(byte type, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
    {
        TupleWriter tw = new TupleWriter(type);
        tw.ReserveType(value1);
        tw.ReserveType(value2);
        tw.ReserveType(value3);
        tw.ReserveType(value4);
        tw.ReserveType(value5);
        tw.ReserveType(value6);
        tw.AllocateBuffer();
        tw.Write(value1);
        tw.Write(value2);
        tw.Write(value3);
        tw.Write(value4);
        tw.Write(value5);
        tw.Write(value6);

        return tw.ToArray();
    }

    public static bool TryDeconstruct<T1, T2, T3, T4, T5, T6>(ReadOnlySpan<byte> key, byte type, out T1 value1, out T2 value2, out T3 value3, out T4 value4, out T5 value5, out T6 value6)
    {
        value1 = default!;
        value2 = default!;
        value3 = default!;
        value4 = default!;
        value5 = default!;
        value6 = default!;
        int position = 1;

        return key[0] == type
            && TupleReader.TryRead(key, ref position, out value1)
            && TupleReader.TryRead(key, ref position, out value2)
            && TupleReader.TryRead(key, ref position, out value3)
            && TupleReader.TryRead(key, ref position, out value4)
            && TupleReader.TryRead(key, ref position, out value5)
            && TupleReader.TryRead(key, ref position, out value6);
    }

    public static (byte Tag, IReadOnlyList<object> Values) DeconstructValues(ReadOnlySpan<byte> key)
    {
        List<object> values = new List<object>();
        int position = 1;
        while (TupleReader.TryReadObject(key, ref position, out object? value))
        {
            values.Add(value);
        }

        return (key[0], values);
    }

    public static string DeconstructAsString(ReadOnlySpan<byte> key)
    {
        int position = 1;
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Tag: {0}", key[0]);
        sb.AppendLine();

        while (TupleReader.TryReadObject(key, ref position, out object? value))
        {
            if (value is null)
            {
                sb.Append(" - Null");
            }
            else
            {
                sb.AppendFormat(" - {0}: {1}", value.GetType().Name, value);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
