using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class WhereIntervalFunction : IQueryNode
{
    private readonly TimeSource from;
    private readonly TimeSource to;

    public WhereIntervalFunction(IQueryNode fromNode, IQueryNode toNode)
    {
        this.from = ((TimeSourceNode)fromNode).TimeSource;
        this.to = ((TimeSourceNode)toNode).TimeSource;
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        if (queryContext.IntervalFnApply)
        {
            throw new YatsdbSyntaxException("INTERVAL function can not apply multiple times.");
        }

        queryContext.IntervalFnApply = true;

        queryObject.From = this.from;
        queryObject.To = this.to;
    }
}
