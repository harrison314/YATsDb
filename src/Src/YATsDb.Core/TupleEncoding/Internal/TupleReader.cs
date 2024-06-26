using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.TupleEncoding.Internal;

internal struct TupleReader
{
    public static bool TryRead<T>(ReadOnlySpan<byte> value, ref int position, out T result)
    {
        result = default!;
        bool rv = false;

        if (typeof(T) == typeof(uint))
        {
            rv = TryReadUint32(value, ref position, out uint v1);
            if (rv) result = (T)(object)v1;
        }
        else if (typeof(T) == typeof(long))
        {
            rv = TryReadInt64(value, ref position, out long v1);
            if (rv) result = (T)(object)v1;
        }
        else if (typeof(T) == typeof(int))
        {
            rv = TryReadInt32(value, ref position, out int v1);
            if (rv) result = (T)(object)v1;
        }
        else if (typeof(T) == typeof(string))
        {
            rv = TryReadString(value, ref position, out string v1);
            if (rv) result = (T)(object)v1;
        }
        else if (typeof(T) == typeof(byte))
        {
            rv = TryReadByte(value, ref position, out byte v1);
            if (rv) result = (T)(object)v1;
        }
        else if (typeof(T) == typeof(bool))
        {
            rv = TryReadBool(value, ref position, out bool v1);
            if (rv) result = (T)(object)v1;
        }
        else if (typeof(T) == typeof(Guid))
        {
            rv = TryReadGuid(value, ref position, out Guid v1);
            if (rv) result = (T)(object)v1;
        }
        else
        {
            throw new InvalidProgramException($"Unsupported type {typeof(T).Name} for tuple");
        }

        return rv;
    }

    public static bool TryReadObject(ReadOnlySpan<byte> value, ref int position, [NotNullWhen(true)] out object? result)
    {
        bool returnValue = false;
        result = null;

        if (position >= value.Length)
        {
            return false;
        }

        switch (value[position])
        {
            case TupleValueType.TupleTrue:
            case TupleValueType.TupleFalse:
                {
                    if (TryReadBool(value, ref position, out bool localResult))
                    {
                        result = localResult;
                        returnValue = true;
                    }
                }
                break;

            case TupleValueType.TupleByte:
                {
                    if (TryReadByte(value, ref position, out byte localResult))
                    {
                        result = localResult;
                        returnValue = true;
                    }
                }
                break;

            case TupleValueType.TupleUint32:
                {
                    if (TryReadUint32(value, ref position, out uint localResult))
                    {
                        result = localResult;
                        returnValue = true;
                    }
                }
                break;
            //case TupleConstants.TupleUint64:
            //    {
            //        if (TryReadUint64(value, ref position, out ulong localResult))
            //        {
            //            result = localResult;
            //            returnValue = true;
            //        }
            //    }
            //    break;

            case TupleValueType.TupleInt64Positive:
            case TupleValueType.TupleInt64Negative:
                {
                    if (TryReadInt64(value, ref position, out long localResult))
                    {
                        result = localResult;
                        returnValue = true;
                    }
                }
                break;

            case TupleValueType.TupleInt32Positive:
            case TupleValueType.TupleInt32Negative:
                {
                    if (TryReadInt32(value, ref position, out int localResult))
                    {
                        result = localResult;
                        returnValue = true;
                    }
                }
                break;

            case TupleValueType.TupleUnicodeString:
                {
                    if (TryReadString(value, ref position, out string localResult))
                    {
                        result = localResult;
                        returnValue = true;
                    }
                }
                break;

            case TupleValueType.TupleGuid:
                {
                    if (TryReadGuid(value, ref position, out Guid localResult))
                    {
                        result = localResult;
                        returnValue = true;
                    }
                }
                break;
            default:
                result = null;
                returnValue = false;
                break;
        }

        return returnValue;
    }

    internal static bool TryReadBool(ReadOnlySpan<byte> value, ref int position, out bool result)
    {
        if (value[position] == TupleValueType.TupleTrue)
        {
            position++;
            result = true;
            return true;
        }

        if (value[position] == TupleValueType.TupleFalse)
        {
            position++;
            result = false;
            return true;
        }

        result = default;
        return false;
    }

    internal static bool TryReadByte(ReadOnlySpan<byte> value, ref int position, out byte result)
    {
        if (value[position] == TupleValueType.TupleByte)
        {
            result = value[position + 1];
            position += 2;
            return true;
        }

        result = default;
        return false;
    }

    internal static bool TryReadUint32(ReadOnlySpan<byte> value, ref int position, out uint result)
    {
        if (value[position] == TupleValueType.TupleUint32)
        {
            if (BinaryPrimitives.TryReadUInt32BigEndian(value.Slice(position + 1), out result))
            {
                position += 5;
                return true;
            }
        }

        result = default;
        return false;
    }

    internal static bool TryReadInt64(ReadOnlySpan<byte> value, ref int position, out long result)
    {
        if (value[position] == TupleValueType.TupleInt64Positive)
        {
            if (BinaryPrimitives.TryReadInt64BigEndian(value.Slice(position + 1), out result))
            {
                position += 9;
                return true;
            }
        }
        else if (value[position] == TupleValueType.TupleInt64Negative)
        {
            if (BinaryPrimitives.TryReadInt64BigEndian(value.Slice(position + 1), out long invertedValue))
            {
                result = -~invertedValue;
                position += 9;
                return true;
            }
        }

        result = default;
        return false;
    }

    internal static bool TryReadInt32(ReadOnlySpan<byte> value, ref int position, out int result)
    {
        if (value[position] == TupleValueType.TupleInt32Positive)
        {
            if (BinaryPrimitives.TryReadInt32BigEndian(value.Slice(position + 1), out result))
            {
                position += 9;
                return true;
            }
        }
        else if (value[position] == TupleValueType.TupleInt32Negative)
        {
            if (BinaryPrimitives.TryReadInt32BigEndian(value.Slice(position + 1), out int invertedValue))
            {
                result = -~invertedValue;
                position += 9;
                return true;
            }
        }

        result = default;
        return false;
    }

    internal static bool TryReadString(ReadOnlySpan<byte> value, ref int position, out string result)
    {
        if (value[position] == TupleValueType.TupleUnicodeString)
        {
            int startPosition = position + 1;
            int len = 0;
            while (value[startPosition + len] != 0)
            {
                len++;
            }

            if (len == 0)
            {
                result = string.Empty;
                position += 2;
            }
            else
            {
                result = Encoding.UTF8.GetString(value.Slice(startPosition, len));
                position = startPosition + len + 1;

            }

            return true;
        }

        result = string.Empty;
        return false;
    }

    internal static bool TryReadGuid(ReadOnlySpan<byte> value, ref int position, out Guid result)
    {
        if (value[position] == TupleValueType.TupleGuid)
        {
            result = new Guid(value.Slice(position + 1));
            position += 17;
            return true;
        }

        result = Guid.Empty;
        return false;
    }
}
