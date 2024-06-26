using Piglet.Parser.Configuration;
using Piglet.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YATsDb.Core.HighLevel.QueryParsing.Nodes;

namespace YATsDb.Core.HighLevel.QueryParsing;

public class QueryParser
{
    private readonly IParser<IQueryNode> parser;

    public QueryParser()
    {
        this.parser = this.BuildParser();
    }

    internal IQueryNode ParseInternal(string query)
    {
        return this.parser.Parse(query);
    }

    public QueryObject Parse(string bucketName, string query)
    {
        try
        {
            IQueryNode selectNode = this.parser.Parse(query);

            QueryContext queryContext = new QueryContext(bucketName);
            QueryObject queryObject = new QueryObject();

            queryObject.From = TimeSource.CreateNull();
            queryObject.To = TimeSource.CreateNull();

            selectNode.ApplyNode(queryContext, queryObject);

            return queryObject;
        }
        catch (YatsdbSyntaxException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new YatsdbSyntaxException($"Error during processing query '{query}'.", ex);
        }
    }

    private IParser<IQueryNode> BuildParser()
    {
        IParserConfigurator<IQueryNode> configurator = ParserFactory.Configure<IQueryNode>();

        ITerminal<IQueryNode> indemnificator = configurator.CreateTerminal("[a-zA-Z_][0-9a-zA-Z_]*", s => new IndemnificatorNode(s));
        ITerminal<IQueryNode> number = configurator.CreateTerminal("\\d+", s => new NumberNode(s));
        ITerminal<IQueryNode> stringLiteral = configurator.CreateTerminal("\"([^\"]+|\\\\\")*\"|'([^']+|\\\\')*'", s => StringNode.FromLiteral(s));
        ITerminal<IQueryNode> timeSpanTerminal = configurator.CreateTerminal("(\\+|-)\\d+(\\.\\d+)?[a-zA-Z]+", s => new TimeSpanNode(s));

        INonTerminal<IQueryNode> selectStatement = configurator.CreateNonTerminal();
        INonTerminal<IQueryNode> projectionStatement = configurator.CreateNonTerminal();
        INonTerminal<IQueryNode> projectionExpression = configurator.CreateNonTerminal();
        INonTerminal<IQueryNode> projectionFn = configurator.CreateNonTerminal();
        INonTerminal<IQueryNode> timeSourceValue = configurator.CreateNonTerminal();

        INonTerminal<IQueryNode> whereStatement = configurator.CreateNonTerminal();
        INonTerminal<IQueryNode> whereFunctions = configurator.CreateNonTerminal();
        INonTerminal<IQueryNode> whereExpression = configurator.CreateNonTerminal();

        INonTerminal<IQueryNode> groupByStatement = configurator.CreateNonTerminal();

        INonTerminal<IQueryNode> limitStatement = configurator.CreateNonTerminal();


        selectStatement.AddProduction("SELECT", projectionStatement, "FROM", indemnificator, whereStatement, groupByStatement, limitStatement)
            .SetReduceFunction(s => new SelectNode(s[1], s[3], s[4], s[5], s[6]));


        projectionExpression.AddProduction(projectionExpression, ",", projectionFn)
            .SetReduceFunction(s => new ListNode(s[0], s[2]));

        projectionExpression.AddProduction(projectionFn)
            .SetReduceFunction(s => new ListNode(s[0]));

        projectionFn.AddProduction(number).SetReduceFunction(s => new ProjectionFunctionNode(s[0]));
        projectionFn.AddProduction(indemnificator, "(", number, ")").SetReduceFunction(s => new ProjectionFunctionNode(s[0], s[2]));

        projectionStatement.AddProduction("*").SetReduceFunction(s => new StarProjectionNode());
        projectionStatement.AddProduction(projectionExpression).SetReduceToFirst();

        timeSourceValue.AddProduction("NOW", "(", ")").SetReduceFunction(_ => TimeSourceNode.Now());
        timeSourceValue.AddProduction("NULL").SetReduceFunction(_ => TimeSourceNode.Null());
        timeSourceValue.AddProduction(timeSpanTerminal).SetReduceFunction(s => TimeSourceNode.Relative(s[0]));
        timeSourceValue.AddProduction(number).SetReduceFunction(s => TimeSourceNode.Absolute(s[0]));
        timeSourceValue.AddProduction(stringLiteral).SetReduceFunction(s => TimeSourceNode.Absolute(s[0]));

        whereFunctions.AddProduction("INTERVAL", "(", timeSourceValue, ",", timeSourceValue, ")").SetReduceFunction(s => new WhereIntervalFunction(s[2], s[4]));
        whereFunctions.AddProduction("TAG", "(", stringLiteral, ")").SetReduceFunction(s => new WhereTagFunction(s[2]));

        whereExpression.AddProduction(whereExpression, "AND", whereFunctions)
            .SetReduceFunction(s => new ListNode(s[0], s[2]));
        whereExpression.AddProduction(whereFunctions)
            .SetReduceFunction(s => new ListNode(s[0]));


        whereStatement.AddProduction("WHERE", whereExpression).SetReduceFunction(s => new WhereStatementNode(s[1]));
        whereStatement.AddProduction().SetReduceFunction(s => NilNode.Instance);

        groupByStatement.AddProduction("GROUP", "BY", timeSpanTerminal).SetReduceFunction(s => new GroupByStatementNode(s[2]));
        groupByStatement.AddProduction().SetReduceFunction(s => NilNode.Instance);

        limitStatement.AddProduction("LIMIT", number, ",", number)
            .SetReduceFunction(s => new LimitStatementNode(s[1], s[3], 1));
        limitStatement.AddProduction("LIMIT", number)
            .SetReduceFunction(s => new LimitStatementNode(NumberNode.Zero, s[1], 1));
        limitStatement.AddProduction("LIMIT", "..", number)
            .SetReduceFunction(s => new LimitStatementNode(NumberNode.Zero, s[2], -1));
        limitStatement.AddProduction().SetReduceFunction(s => NilNode.Instance);

        return configurator.CreateParser();
    }
}
