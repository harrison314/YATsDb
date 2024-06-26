using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class TimeSpanNode : IQueryNode
{
    public TimeSpan TimeSpan
    {
        get;
    }

    public TimeSpanNode(TimeSpan ts)
    {
        this.TimeSpan = ts;

    }
    public TimeSpanNode(long ms)
    {
        this.TimeSpan = TimeSpan.FromMilliseconds(ms);
    }

    public TimeSpanNode(string timeSpan)
    {
        int unitIndex = 1;
        while (unitIndex < timeSpan.Length)
        {
            char c = timeSpan[unitIndex];
            if (char.IsDigit(c) || c == '.')
            {
                unitIndex++;
            }
            else
            {
                break;
            }
        }

        double numberValue = double.Parse(timeSpan.AsSpan(1, unitIndex - 1), System.Globalization.CultureInfo.InvariantCulture);
        this.TimeSpan = timeSpan[unitIndex..].ToLowerInvariant() switch
        {
            "ms" => TimeSpan.FromMilliseconds(numberValue),
            "s" => TimeSpan.FromSeconds(numberValue),
            "m" => TimeSpan.FromMinutes(numberValue),
            "h" => TimeSpan.FromHours(numberValue),
            "d" => TimeSpan.FromDays(numberValue),
            "w" => TimeSpan.FromDays(numberValue * 7.0),
            "y" => TimeSpan.FromDays(numberValue * 365.0),
            string unit => throw new FormatException($"Unit '{unit}' not recognized.")
        };

        if (timeSpan[0] == '-')
        {
            this.TimeSpan = -this.TimeSpan;
        }
    }

    public TimeSpanNode Negate(TimeSpanNode timeSpanNode)
    {
        return new TimeSpanNode(-timeSpanNode.TimeSpan);
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        throw new NotSupportedException();
    }

    public override string ToString()
    {
        return this.TimeSpan.ToString();
    }
}
