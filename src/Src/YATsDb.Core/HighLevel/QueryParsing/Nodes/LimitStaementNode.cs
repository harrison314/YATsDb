namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class LimitStatementNode : IQueryNode
{
    private readonly int skip;
    private readonly int take;

    public LimitStatementNode(IQueryNode skip, IQueryNode take, int sign)
    {
        this.skip = ((NumberNode)skip).IntNumber;
        this.take = sign * ((NumberNode)take).IntNumber;
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        queryObject.Skip = this.skip;
        queryObject.Take = this.take;
    }

    public override string ToString()
    {
        if (this.take == -1)
        {
            return "LIMIT TO LAST";
        }

        return $"LIMIT {this.skip}, {this.take}";
    }
}
