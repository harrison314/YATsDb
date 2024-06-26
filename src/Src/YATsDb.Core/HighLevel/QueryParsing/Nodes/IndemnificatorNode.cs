using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YATsDb.Core.HighLevel.QueryParsing;

namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class IndemnificatorNode : IQueryNode
{
    public string Indemnificator
    {
        get;
    }

    public IndemnificatorNode(string indemnificator)
    {
        this.Indemnificator = indemnificator;
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        throw new NotSupportedException();
    }

    public override string ToString()
    {
        return this.Indemnificator;
    }
}
