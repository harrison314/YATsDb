using System.Text;
using YATsDb.Core.HighLevel.QueryParsing;

namespace YATsDb.Core.HighLevel.QueryParsing.Nodes;

internal class SelectNode : IQueryNode
{
    private readonly IQueryNode projection;
    private readonly IQueryNode measurementName;
    private readonly IQueryNode where;
    private readonly IQueryNode groupBy;
    private readonly IQueryNode limit;

    public SelectNode(IQueryNode projection, IQueryNode measurementName, IQueryNode where, IQueryNode groupBy, IQueryNode limit)
    {
        this.projection = projection;
        this.measurementName = measurementName;
        this.where = where;
        this.groupBy = groupBy;
        this.limit = limit;
    }

    public void ApplyNode(QueryContext queryContext, QueryObject queryObject)
    {
        queryObject.BucketName = queryContext.BucketName.AsMemory();
        queryObject.MeasurementName = ((IndemnificatorNode)this.measurementName).Indemnificator.AsMemory();

        this.limit.ApplyNode(queryContext, queryObject);
        this.groupBy.ApplyNode(queryContext, queryObject);
        this.where.ApplyNode(queryContext, queryObject);

        if (this.projection is ListNode listNode)
        {
            foreach (IQueryNode projectionNode in listNode.Nodes)
            {
                projectionNode.ApplyNode(queryContext, queryObject);
            }
        }
        else
        {
            if (this.groupBy is not NilNode)
            {
                throw new YatsdbSyntaxException("SELECT with * can not use Group BY");
            }

            this.projection.ApplyNode(queryContext, queryObject);
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("SELECT ");
        if (this.projection is ListNode listNode)
        {
            bool isFirst = true;
            foreach (IQueryNode node in listNode.Nodes)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append(" ,");
                }

                sb.Append(node.ToString());
            }
        }
        else
        {
            sb.Append(this.projection.ToString());
        }

        sb.Append(" FROM ").Append(this.measurementName.ToString());

        if (this.where != NilNode.Instance)
        {
            sb.Append(' ').Append(this.where.ToString());
        }

        if (this.groupBy != NilNode.Instance)
        {
            sb.Append(' ').Append(this.groupBy.ToString());
        }

        if (this.limit != NilNode.Instance)
        {
            sb.Append(' ').Append(this.limit.ToString());
        }

        return sb.ToString();
    }
}
