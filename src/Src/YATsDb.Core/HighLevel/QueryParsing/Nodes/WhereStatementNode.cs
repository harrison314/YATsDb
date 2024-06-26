using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class WhereStatementNode : IQueryNode
{
    private List<IQueryNode> nodes;
    public WhereStatementNode(IQueryNode whereList)
    {
        this.nodes = ((ListNode)whereList).Nodes;
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        foreach (IQueryNode node in this.nodes)
        {
            node.ApplyNode(queryContext, queryObject);
        }
    }

    public override string ToString()
    {
        return $"WHERE {string.Join(" AND ", this.nodes)}";
    }
}
