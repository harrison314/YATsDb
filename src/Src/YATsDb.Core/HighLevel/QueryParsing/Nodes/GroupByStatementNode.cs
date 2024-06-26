using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class GroupByStatementNode : IQueryNode
{
    private readonly TimeSpan groupByTimespan;

    public GroupByStatementNode(IQueryNode groupBy)
    {
        TimeSpanNode timeSpanNode = (TimeSpanNode)groupBy;
        this.groupByTimespan = timeSpanNode.TimeSpan > TimeSpan.Zero ? timeSpanNode.TimeSpan : -timeSpanNode.TimeSpan;
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        System.Diagnostics.Debug.Assert(!queryObject.GroupByMs.HasValue);
        queryObject.GroupByMs = (long)this.groupByTimespan.TotalMilliseconds;
    }

    public override string ToString()
    {
        return $"GROUP BY '{this.groupByTimespan}'";
    }
}
