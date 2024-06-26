using System.Runtime.InteropServices;

namespace YATsDb.Core.HighLevel;

public struct DbValue
{
    public enum Type : int
    {
        Null,
        Double,
        Long,
        DateTimeOffset,
        String
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct DbValueUnion
    {
        [FieldOffset(0)]
        public double valueD;
        [FieldOffset(0)]
        public long valueL;
    }

    private Type type;
    private DbValueUnion valueUnion;
    private string? valueS;

    public Type DbType
    {
        get => type;
    }

    public DbValue()
    {
        type = Type.Null;
    }

    public object? GetValue()
    {
        return type switch
        {
            Type.Null => null,
            Type.Double => valueUnion.valueD,
            Type.Long => valueUnion.valueL,
            Type.DateTimeOffset => DateTimeOffset.FromUnixTimeMilliseconds(valueUnion.valueL),
            Type.String => valueS!,
            _ => throw new InvalidProgramException($"Enum value {type} is not supported.")
        };
    }

    public static DbValue CreateFromNull()
    {
        return new DbValue();
    }

    public static DbValue CreateFromDouble(double value, bool nullIfNan)
    {
        if (nullIfNan && double.IsNaN(value))
        {
            return new DbValue();
        }

        return new DbValue()
        {
            type = Type.Double,
            valueUnion =
            {
                valueD = value
            }
        };
    }

    public static DbValue CreateFromLong(long value)
    {
        return new DbValue()
        {
            type = Type.Long,
            valueUnion =
            {
                valueL = value
            }
        };
    }

    public static DbValue CreateFromLong(long value, bool unixTimespanInMs)
    {
        return new DbValue()
        {
            type = unixTimespanInMs ? Type.DateTimeOffset : Type.Long,
            valueUnion =
            {
                valueL = value
            }
        };
    }

    public static DbValue CreateFromDateTimeOffset(DateTimeOffset value)
    {
        return new DbValue()
        {
            type = Type.DateTimeOffset,
            valueUnion =
            {
                valueL = value.ToUnixTimeMilliseconds()
            }
        };
    }

    public static DbValue CreateFromString(string? value)
    {
        if (value == null)
        {
            return new DbValue();
        }

        return new DbValue()
        {
            type = Type.String,
            valueS = value
        };
    }

    public T Map<T>(Func<T> mapNull,
        Func<double, T> mapDouble,
        Func<long, T> mapLong,
        Func<DateTimeOffset, T> mapDateTimeOffset,
        Func<string, T> mapString)
    {
        return type switch
        {
            Type.Null => mapNull(),
            Type.Double => mapDouble(valueUnion.valueD),
            Type.Long => mapLong(valueUnion.valueL),
            Type.DateTimeOffset => mapDateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(valueUnion.valueL)),
            Type.String => mapString(valueS!),
            _ => throw new InvalidProgramException($"Enum value {type} is not supported.")
        };
    }

    public override string ToString()
    {
        object? value = GetValue();
        if (value == null)
        {
            return "NULL";
        }
        else
        {
            return value.ToString() ?? "NULL";
        }
    }
}
