namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class NumberNode : IQueryNode
{
    public static NumberNode Zero = new NumberNode(0);
    
    public long Number
    {
        get;
    }

    public int IntNumber
    {
        get => Convert.ToInt32(this.Number);
    }

    public NumberNode(string number)
    {
        this.Number = long.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
    }

    public NumberNode(long number)
    {
        this.Number = number;
    }

    public override string ToString()
    {
        return this.Number.ToString();
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        throw new NotSupportedException();
    }
}