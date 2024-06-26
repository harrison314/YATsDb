using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YATsDb.Core.HighLevel;
using YATsDb.Core.HighLevel.QueryParsing;

namespace YATsDb.Core.Services;

public class DalServices : IDalServices
{
    private readonly static QueryParser queryParser = new QueryParser();

    private readonly IYatsdbHighLevelStorage storage;
    private readonly ICache cache;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<DalServices> logger;

    public DalServices(IYatsdbHighLevelStorage storage,
        ICache cache,
        TimeProvider timeProvider,
        ILogger<DalServices> logger)
    {
        this.storage = storage;
        this.cache = cache;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public void InsertLines(string bucketName, string lines)
    {
        this.logger.EnteringToInsertLines(bucketName);

        ArgumentException.ThrowIfNullOrEmpty(bucketName, nameof(bucketName));
        if (string.IsNullOrWhiteSpace(lines))
        {
            throw new YatsdbSyntaxException("lines can not be empty string");
        }

        LineProtocolParser lineProtocolParser = new LineProtocolParser(this.timeProvider);
        List<HighLevelInputDataPoint> dataPoints = lineProtocolParser.Parse(lines);

        this.storage.Insert(bucketName.AsMemory(),
            dataPoints);

        this.logger.LogInsertedCountLines(dataPoints.Count, bucketName);
    }

    public List<object?[]> Query(string bucketName, string query, QueryParameters parameters)
    {
        this.logger.EnteringToQuery(bucketName);

        ArgumentException.ThrowIfNullOrEmpty(bucketName, nameof(bucketName));
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new YatsdbSyntaxException("query can not be empty string");
        }

        Stopwatch stp = Stopwatch.StartNew();
        string queryCacheKey = string.Concat("Query:", bucketName, ":", query);
        PreparedQueryObject? preparedQuery = this.cache.GetOrCreate(queryCacheKey, () =>
        {
            QueryObject qo = queryParser.Parse(bucketName, query);
            PreparedQueryObject pq = this.storage.PrepareQuery(qo);
            return (pq, TimeSpan.FromMinutes(3.2));
        });

        Debug.Assert(preparedQuery != null);

        List<DbValue[]> raw = this.storage.ExecuteQuery(preparedQuery);
        stp.Stop();

        this.logger.LogExecutedQuery(query, stp.Elapsed);

        List<object?[]> result = new List<object?[]>(raw.Count);
        foreach (DbValue[] value in raw)
        {
            object?[] line = new object?[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                line[i] = this.TransformObject(ref value[i], parameters);
            }

            result.Add(line);
        }

        return result;
    }

    private object? TransformObject(ref DbValue value, QueryParameters queryParameters)
    {
        if (queryParameters.TimeRepresentation == TimeRepresentation.DateTimeOffset)
        {
            return value.GetValue();
        }

        if (value.DbType == DbValue.Type.DateTimeOffset)
        {
            DateTimeOffset dateTimeOffset = (DateTimeOffset)value.GetValue()!;
            if (queryParameters.TimeRepresentation == TimeRepresentation.UnixTimestamp)
            {
                return dateTimeOffset.ToUnixTimeSeconds();
            }

            if (queryParameters.TimeRepresentation == TimeRepresentation.UnixTimestampInMs)
            {
                return dateTimeOffset.ToUnixTimeMilliseconds();
            }
        }

        return value.GetValue();
    }
}
