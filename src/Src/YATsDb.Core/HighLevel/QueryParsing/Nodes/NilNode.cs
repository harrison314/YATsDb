using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.HighLevel.QueryParsing.Nodes
{
    internal class NilNode : IQueryNode
    {
        public static readonly NilNode Instance = new NilNode();

        private NilNode()
        {
        }

        public override string ToString()
        {
            return string.Empty;
        }

        public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
        {
            //NOP
        }
    }
}
