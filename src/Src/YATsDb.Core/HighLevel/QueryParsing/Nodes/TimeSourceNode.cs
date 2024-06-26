using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YATsDb.Core.HighLevel.QueryParsing;

namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class TimeSourceNode : IQueryNode
{
    public TimeSource TimeSource
    {
        get;
        private set;
    }

    private TimeSourceNode()
    {
    }

    public static TimeSourceNode Null()
    {
        return new TimeSourceNode()
        {
            TimeSource = TimeSource.CreateNull()
        };
    }

    public static TimeSourceNode Now()
    {
        return new TimeSourceNode()
        {
            TimeSource = TimeSource.CreateRelative(TimeSpan.Zero)
        };
    }

    public static TimeSourceNode Relative(IQueryNode node)
    {
        TimeSpanNode tsNode = (TimeSpanNode)node;
        return new TimeSourceNode()
        {
            TimeSource = TimeSource.CreateRelative(tsNode.TimeSpan)
        };
    }

    public static TimeSourceNode Absolute(IQueryNode node)
    {
        if (node is NumberNode numberNode)
        {
            return new TimeSourceNode()
            {
                TimeSource = TimeSource.CreateConstant(DateTimeOffset.FromUnixTimeMilliseconds(numberNode.Number))
            };
        }

        if (node is StringNode stringNode)
        {
            return new TimeSourceNode()
            {
                TimeSource = TimeSource.CreateConstant(ParserDateTime(stringNode.Value))
            };
        }

        throw new InvalidProgramException();
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        throw new NotSupportedException();
    }

    private static DateTimeOffset ParserDateTime(string value)
    {
        DateTimeOffset result;

        if (DateTimeOffset.TryParseExact(value,
            "O",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AllowWhiteSpaces,
            out result))
        {
            return result;
        }

        if (DateTimeOffset.TryParseExact(value,
            "s",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AllowWhiteSpaces,
            out result))
        {
            return result;
        }

        if (DateTimeOffset.TryParse(value,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AllowWhiteSpaces,
            out result))
        {
            return result;
        }

        throw new YatsdbSyntaxException($"Excepted datetime offset in '{value}'");
    }
}
