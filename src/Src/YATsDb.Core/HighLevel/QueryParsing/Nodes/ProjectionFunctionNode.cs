using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class ProjectionFunctionNode : IQueryNode
{
    private string functionName;
    private int index;

    public ProjectionFunctionNode(IQueryNode identifier, IQueryNode number)
    {
        this.functionName = ((IndemnificatorNode)identifier).Indemnificator;
        this.index = ((NumberNode)number).IntNumber;
    }

    public ProjectionFunctionNode(IQueryNode number)
    {
        this.functionName = "identity";
        this.index = ((NumberNode)number).IntNumber;
    }

    public override string ToString()
    {
        return $"{this.functionName}({this.index})";
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        if (queryObject.Aggregations == null)
        {
            queryObject.Aggregations = new List<AggregationQuery>();
        }

        queryObject.Aggregations.Add(new AggregationQuery(this.index, this.TranslateFnName(this.functionName)));
    }

    private AggregationType TranslateFnName(string fnName)
    {
        return fnName.ToLowerInvariant() switch
        {
            "variance" => AggregationType.Variance,
            "sum" => AggregationType.Sum,
            "count" => AggregationType.Count,
            "avg" => AggregationType.Avg,
            "identity" => AggregationType.Identity,
            "max" => AggregationType.Max,
            "min" => AggregationType.Min,
            "stddev" => AggregationType.StdDev,
            "remedian" => AggregationType.Remedian,
            "sign" => AggregationType.Sign,
            "change_per_sec" => AggregationType.ChangePerSec,
            "change_per_hour" => AggregationType.ChangePerHour,
            "change_per_day" => AggregationType.ChangePerDay,
            _ => throw new YatsdbSyntaxException($"Function with name {fnName} not found.")
        };
    }
}
