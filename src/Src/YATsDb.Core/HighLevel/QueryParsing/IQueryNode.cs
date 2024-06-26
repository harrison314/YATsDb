using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.HighLevel.QueryParsing;

internal interface IQueryNode
{
    void ApplyNode(QueryContext queryContext, QueryObject queryObject);
}
