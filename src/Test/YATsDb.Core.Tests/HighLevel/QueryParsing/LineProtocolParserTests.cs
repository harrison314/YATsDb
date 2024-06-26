using YATsDb.Core.HighLevel.QueryParsing;

namespace YATsDb.Core.Tests.HighLevel.QueryParsing;

public class LineProtocolParserTests
{
    [Theory]
    [InlineData("my_measurement,tag 12.0 4589651", 1)]
    [InlineData("my_measurement,tag 12.0", 1)]
    [InlineData("my_measurement,tag 12.0,0.78 4589651", 1)]
    [InlineData("my_measurement 12.0 4589651", 1)]
    [InlineData("my_measurement 12.0", 1)]
    [InlineData("my_measurement,tag 12.0,NULL,8.2 4589651", 1)]
    [InlineData("my_measurement,tag 12.0,null,8.2 4589651", 1)]
    [InlineData("my_measurement,tag 120 4589651", 1)]
    [InlineData("my_measurement,measurement1 1 4589651\nmy_measurement,measurement2 47.78,NULL 4589651", 2)]
    public void LineProtocolParser_Parse(string lines, int expected)
    {
        LineProtocolParser parser = new LineProtocolParser(TimeProvider.System);

        List<YATsDb.Core.HighLevel.HighLevelInputDataPoint> result = parser.Parse(lines);
        Assert.Equal(expected, result.Count);
    }

    [Fact]
    public void LineProtocolParser_ParseExact()
    {
        LineProtocolParser parser = new LineProtocolParser(TimeProvider.System);
        List<YATsDb.Core.HighLevel.HighLevelInputDataPoint> result = parser.Parse("myMeasurement,city/street/458 1,2,NULL 14589");

        Assert.Equal("myMeasurement", result[0].MeasurementName.ToString());
        Assert.Equal("city/street/458", result[0].Tag.ToString());
        Assert.Equal(1.0, result[0].Values.Span[0]);
        Assert.Equal(2.0, result[0].Values.Span[1]);
        Assert.True(double.IsNaN(result[0].Values.Span[2]));
        Assert.Equal(14589L, result[0].UnixTimeStampInMs);
    }
}