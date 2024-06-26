using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YATsDb.Core.HighLevel.QueryParsing;
using YATsDb.Core.HighLevel;

namespace YATsDb.Core.Tests.HighLevel.QueryParsing;

public class QueryParserTest
{
    [Theory]
    [InlineData("SELECT * FROM myMeasurement")]
    [InlineData("SELECT * FROM myMeasurement LIMIT 100")]
    [InlineData("SELECT * FROM myMeasurement LIMIT 0,100")]
    [InlineData("SELECT * FROM myMeasurement LIMIT 0, 100")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(10, 1555)")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(10, 1555) AND TAG('hello')")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(10, 1555) LIMIT 500")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(10, 1555) LIMIT .. 1")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(10, 1555) LIMIT ..1")]
    [InlineData("SELECT 0, 1, 2, 3 FROM myMeasurement")]
    [InlineData("SELECT MIN(0), MAX(0), AVG(0), COUNT(0) FROM myMeasurement")]
    [InlineData("SELECT COUNT(1) FROM myMeasurement LIMIT 500")]
    [InlineData("SELECT COUNT(1) FROM myMeasurement GROUP BY +15s LIMIT 500")]
    [InlineData("SELECT COUNT(1) FROM myMeasurement WHERE TAG('hello') GROUP BY +15s LIMIT 0,500")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(NULL, -7d)")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(-1.2h, +7d)")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(NULL, NOW())")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(NULL, -1ms)")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(NULL, -1s)")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(NULL, -1m)")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(NULL, -1h)")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(NULL, -1d)")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(NULL, -1w)")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL(NULL, -1y)")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL('2012-06-12', NOW())")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL('2012-06-12T08:33:12', NULL)")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL('2012-06-12 8:30:12', NULL)")]
    [InlineData("SELECT * FROM myMeasurement WHERE INTERVAL('2012-06-12 08:33:12.478', NULL)")]
    [InlineData("SELECT 0, 1 FROM myMeasurement")]
    [InlineData("SELECT MAX(0) FROM myMeasurement")]
    [InlineData("SELECT MIN(0) FROM myMeasurement")]
    [InlineData("SELECT AVG(0) FROM myMeasurement")]
    [InlineData("SELECT COUNT(0), COUNT(1) FROM myMeasurement")]
    [InlineData("SELECT VARIANCE(0), COUNT(1) FROM myMeasurement")]
    [InlineData("SELECT variance(0) FROM myMeasurement")]
    [InlineData("SELECT STDDEV(0) FROM myMeasurement")]
    [InlineData("SELECT STDDEV(0), MIN(0), MAX(0),AVG(0), COUNT(0), VARIANCE(0)  FROM myMeasurement GROUP BY +1d")]
    [InlineData("SELECT STDDEV(0), MIN(0), MAX(0),AVG(0), COUNT(0), VARIANCE(0)  FROM myMeasurement WHERE TAG('mytag') GROUP BY +1d")]
    [InlineData("SELECT STDDEV(0), MIN(0), MAX(0),AVG(0), COUNT(0), VARIANCE(0)  FROM myMeasurement WHERE TAG('mytag') AND INTERVAL(-1y, NOW()) GROUP BY +1d")]
    [InlineData("SELECT REMEDIAN(0), COUNT(1) FROM myMeasurement")]
    [InlineData("SELECT SIGN(0) FROM myMeasurement")]
    [InlineData("SELECT CHANGE_PER_SEC(0) FROM myMeasurement")]
    [InlineData("SELECT CHANGE_PER_HOUR(0) FROM myMeasurement")]
    [InlineData("SELECT CHANGE_PER_DAY(0) FROM myMeasurement")]
    public void TestSimpleParsingQuery(string query)
    {
        QueryParser queryParser = new QueryParser();

        var node = queryParser.ParseInternal(query);

        Assert.IsType<YATsDb.Core.HighLevel.QueryParsing.Nodes.SelectNode>(node);

        QueryContext queryContext = new QueryContext("bucket1");
        QueryObject queryObject = new QueryObject();

        node.ApplyNode(queryContext, queryObject);
    }

    [Fact]
    public void TestSimpleQuery()
    {
        QueryParser queryParser = new QueryParser();

        var node = queryParser.ParseInternal("SELECT * FROM behhlo LIMIT 12");

        Assert.IsType<YATsDb.Core.HighLevel.QueryParsing.Nodes.SelectNode>(node);
    }
}
