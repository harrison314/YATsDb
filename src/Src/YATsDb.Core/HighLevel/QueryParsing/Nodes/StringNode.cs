namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class StringNode : IQueryNode
{
    public string Value
    {
        get;
    }

    public static StringNode FromLiteral(string literal)
    {
        if (literal[0] is '"' or '\'')
        {
            return new StringNode(literal[1..^1]);
        }

        throw new InvalidProgramException("value is not string with ' or \"");
    }

    public StringNode(string value)
    {
        this.Value = value;
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        throw new NotSupportedException();
    }

    public override string ToString()
    {
        return $"'{this.Value}'";
    }
}