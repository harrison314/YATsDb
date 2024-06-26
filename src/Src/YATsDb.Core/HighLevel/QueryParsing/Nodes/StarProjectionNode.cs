using YATsDb.Core.HighLevel.QueryParsing;

namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class StarProjectionNode : IQueryNode
{
    public StarProjectionNode()
    {

    }

    public override string ToString()
    {
        return "*";
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        queryObject.Aggregations = null;
    }
}