using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tenray.ZoneTree;
using Xunit.Abstractions;
using YATsDb.Core.HighLevel;
using YATsDb.Core.LowLevel;

namespace YATsDb.Core.Tests.HighLevel;

public sealed class YatsdbHighLevelStorageTests : IDisposable
{
    private string directory;
    private readonly ITestOutputHelper output;

    public YatsdbHighLevelStorageTests(ITestOutputHelper output)
    {
        this.directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.directory);
        this.output = output;
    }

    public void Dispose()
    {
        Directory.Delete(this.directory, true);
    }

    [Fact]
    public void CreateBucket()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);
        YatsdbHighLevelStorage hlStorage = new YatsdbHighLevelStorage(storage, TimeProvider.System, new NullLogger<YatsdbHighLevelStorage>());

        hlStorage.CreateBucket("bucket1", null, DateTimeOffset.UtcNow);
        hlStorage.CreateBucket("bucket2", "orem ipsum dolor sit amet, consectetur adipiscing elit. Donec nisi erat, dapibus sit amet metus in, pretium porttitor ante. Fusce quis augue vel tellus vestibulum semper. Sed eget odio turpis.", DateTimeOffset.UtcNow);
        hlStorage.CreateBucket("bucket1_test_long_name", null, DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Insert()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);
        YatsdbHighLevelStorage hlStorage = new YatsdbHighLevelStorage(storage, TimeProvider.System, new NullLogger<YatsdbHighLevelStorage>());

        hlStorage.CreateBucket("bucket1", null, DateTimeOffset.UtcNow);

        hlStorage.Insert("bucket1".AsMemory(), new HighLevelInputDataPoint[]
        {
            new HighLevelInputDataPoint("home_data".AsMemory(),4589L, new double[]{154.0},"".AsMemory()),
            new HighLevelInputDataPoint("home_data".AsMemory(),4574L, new double[]{156.0, 458.4, 12.0},"other".AsMemory()),
            new HighLevelInputDataPoint("home_data".AsMemory(),5001, new double[]{154.0},"some_tag".AsMemory()),
            new HighLevelInputDataPoint("home_data".AsMemory(),5002, new double[]{154.0},"".AsMemory())
        });

        hlStorage.Insert("bucket1".AsMemory(), new HighLevelInputDataPoint[]
        {
            new HighLevelInputDataPoint("home_data".AsMemory(),5003L, new double[]{145.0, 4.0},"nope".AsMemory()),
        });

        hlStorage.Insert("bucket1".AsMemory(), new HighLevelInputDataPoint[]
        {
            new HighLevelInputDataPoint("home_data".AsMemory(),5020L, new double[]{-48},"".AsMemory()),
            new HighLevelInputDataPoint("home_data_2".AsMemory(),5020L, new double[]{12.5},"hlmn".AsMemory()),
            new HighLevelInputDataPoint("home_data_3".AsMemory(),5040L, new double[]{12.5},"".AsMemory()),
            new HighLevelInputDataPoint("home_data_2".AsMemory(),5080L, new double[]{12.5},"noloko".AsMemory()),
        });
    }


    [Fact]
    public void Query_Agregate_All()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);
        YatsdbHighLevelStorage hlStorage = new YatsdbHighLevelStorage(storage, TimeProvider.System, new NullLogger<YatsdbHighLevelStorage>());

        hlStorage.CreateBucket("bucket1", null, DateTimeOffset.UtcNow);

        hlStorage.Insert("bucket1".AsMemory(), new HighLevelInputDataPoint[]
        {
            new HighLevelInputDataPoint("home_data".AsMemory(),10, new double[]{10.0},"".AsMemory()),
            new HighLevelInputDataPoint("home_data".AsMemory(),20, new double[]{15.0, 10.0},"".AsMemory()),
            new HighLevelInputDataPoint("home_data".AsMemory(),30, new double[]{26.0, 30.0},"Home".AsMemory()),
        });


        PreparedQueryObject pq1 = hlStorage.PrepareQuery(new QueryObject()
        {
            BucketName = "bucket1".AsMemory(),
            MeasurementName = "home_data".AsMemory(),
            Aggregations = new List<AggregationQuery>()
            {
                new AggregationQuery(0, AggregationType.Min),
                new AggregationQuery(0, AggregationType.Max),
                new AggregationQuery(0, AggregationType.Avg),
                new AggregationQuery(0, AggregationType.Count),
                new AggregationQuery(1, AggregationType.Avg),
                new AggregationQuery(1, AggregationType.Count),
            },
            From = TimeSource.CreateNull(),
            To = TimeSource.CreateNull(),
        });

        List<DbValue[]> result = hlStorage.ExecuteQuery(pq1);
        this.PrintDbSetup(result);

        Assert.Equal(1, result.Count);
        Assert.Equal(10.0, (double)result[0][0].GetValue());
        Assert.Equal(26.0, (double)result[0][1].GetValue());
        Assert.Equal(17.0, (double)result[0][2].GetValue());
        Assert.Equal(3L, (long)result[0][3].GetValue());
        Assert.Equal(20.0, (double)result[0][4].GetValue());
        Assert.Equal(2L, (long)result[0][5].GetValue());
    }

    [Fact]
    public void Query_Agregate_ByTime()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);
        YatsdbHighLevelStorage hlStorage = new YatsdbHighLevelStorage(storage, TimeProvider.System, new NullLogger<YatsdbHighLevelStorage>());

        hlStorage.CreateBucket("bucket1", null, DateTimeOffset.UtcNow);

        hlStorage.Insert("bucket1".AsMemory(), new HighLevelInputDataPoint[]
        {
            new HighLevelInputDataPoint("home_data".AsMemory(),10, new double[]{10.0},"".AsMemory()),
            new HighLevelInputDataPoint("home_data".AsMemory(),12, new double[]{10.0},"".AsMemory()),
            new HighLevelInputDataPoint("home_data".AsMemory(),20, new double[]{15.0, 10.0},"".AsMemory()),
            new HighLevelInputDataPoint("home_data".AsMemory(),28, new double[]{15.0, 10.0},"".AsMemory()),
            new HighLevelInputDataPoint("home_data".AsMemory(),30, new double[]{26.0, 30.0},"Home".AsMemory()),
            new HighLevelInputDataPoint("home_data".AsMemory(),35, new double[]{26.0, 30.0},"Home".AsMemory()),
        });


        PreparedQueryObject pq1 = hlStorage.PrepareQuery(new QueryObject()
        {
            BucketName = "bucket1".AsMemory(),
            MeasurementName = "home_data".AsMemory(),
            Aggregations = new List<AggregationQuery>()
            {
                new AggregationQuery(0, AggregationType.Min),
                new AggregationQuery(0, AggregationType.Max),
                new AggregationQuery(0, AggregationType.Avg),
                new AggregationQuery(0, AggregationType.Count),
                new AggregationQuery(1, AggregationType.Avg),
                new AggregationQuery(1, AggregationType.Count),
            },
            From = TimeSource.CreateNull(),
            To = TimeSource.CreateNull(),
            GroupByMs = 10L
        });

        List<DbValue[]> result = hlStorage.ExecuteQuery(pq1);
        this.PrintDbSetup(result);

        Assert.Equal(3, result.Count);
    }

    private void PrintDbSetup(List<DbValue[]> rows)
    {
        int index = 0;
        StringBuilder sb = new StringBuilder();
        foreach (DbValue[] row in rows)
        {
            sb.Clear();
            sb.AppendFormat("{0}. ", index++);

            foreach (DbValue cell in row)
            {
                if (cell.DbType == DbValue.Type.String)
                {
                    sb.AppendFormat("'{0}' ", cell.ToString());
                }
                else
                {
                    sb.AppendFormat("{0} ", cell.ToString());
                }
            }

            this.output.WriteLine(sb.ToString());
        }
    }
}
