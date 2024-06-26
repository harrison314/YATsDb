using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YATsDb.Core.HighLevel.QueryParsing;

namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class ListNode : IQueryNode
{
    public List<IQueryNode> Nodes { get; set; }
    public ListNode()
    {
        this.Nodes = new List<IQueryNode>();
    }

    public ListNode(IQueryNode item1)
    {
        this.Nodes = new List<IQueryNode>();
        this.AddInternal(item1);
    }

    public ListNode(IQueryNode item1, IQueryNode item2)
    {
        this.Nodes = new List<IQueryNode>();
        this.AddInternal(item1);
        this.AddInternal(item2);
    }

    public ListNode(IQueryNode item1, IQueryNode item2, IQueryNode item3)
    {
        this.Nodes = new List<IQueryNode>();
        this.AddInternal(item1);
        this.AddInternal(item2);
        this.AddInternal(item3);
    }

    private void AddInternal(IQueryNode node)
    {
        System.Diagnostics.Debug.Assert(node != null);

        if (node is ListNode listNode)
        {
            this.Nodes.AddRange(listNode.Nodes);
        }
        else
        {
            this.Nodes.Add(node);
        }
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        throw new NotSupportedException();
    }
}
