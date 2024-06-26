namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class WhereTagFunction : IQueryNode
{
    private readonly string? tag;
    public WhereTagFunction(IQueryNode stringNode)
    {
        this.tag = stringNode switch
        {
            StringNode sn => sn.Value,
            NilNode _ => null,
            _ => throw new InvalidProgramException()
        };
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        if (queryContext.TagFnApply)
        {
            throw new YatsdbSyntaxException("TAG function can not apply multiple times.");
        }

        queryContext.TagFnApply = true;

        if (!string.IsNullOrEmpty(this.tag))
        {
            queryObject.RequestedTag = this.tag.AsMemory();
        }
    }
}