using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;
using YATsDb.Core.LowLevel;
using YATsDb.Core.TupleEncoding;
using YATsDb.Core.Utils;

namespace YATsDb.Core.HighLevel;

public class YatsdbHighLevelStorage : IYatsdbHighLevelStorage
{
    private readonly IYatsdbLowLevelStorage lowLevelStorage;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<YatsdbHighLevelStorage> logger;

    public YatsdbHighLevelStorage(IYatsdbLowLevelStorage lowLevelStorage,
        TimeProvider timeProvider,
        ILogger<YatsdbHighLevelStorage> logger)
    {
        this.lowLevelStorage = lowLevelStorage;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public void CreateBucket(string bucket, string? description, DateTimeOffset cratedAt)
    {
        this.logger.CreateBucket_LogEntering(bucket);

        BucketCreationData bucketCreationData = new BucketCreationData()
        {
            CreateTimeUnixTimestampInMs = cratedAt.ToUnixTimeMilliseconds(),
            Description = description,
            Name = bucket
        };

        if (!this.lowLevelStorage.TryCreateBucket(bucketCreationData, out _))
        {
            this.logger.CreateBucket_LogBucketAlreadyExists(bucket);
            throw new YatsdbDataException($"Bucket {bucket} alerady exists.");
        }

        this.logger.CreateBucket_LogCreateBucket(bucket);
    }

    public void DeleteBucket(string bucket, DateTimeOffset now)
    {
        this.logger.DeleteBucket_LogEntering(bucket);

        if (this.lowLevelStorage.TryRemoveBucket(bucket, out uint bucketId))
        {
            this.logger.LogBucketDoesNotExists(bucket);
            throw new YatsdbDataException($"Bucket {bucketId} does not exists.");
        }

        byte[] removeBucketKey = TupleEncoder.Create(DataType.ApplicationQueue,
            QueueDataType.BucketRemover,
        now.ToUnixTimeMilliseconds(),
        Random.Shared.NextUint32());

        byte[] removeBucketValue = TupleEncoder.Create(QueueDataType.BucketRemover, bucketId);

        this.lowLevelStorage.Db.Upsert(removeBucketKey, removeBucketValue);

        this.logger.DeleteBucket_LogBuckedRemoved(bucket);
    }

    public List<HighLevelBucketInfo> GetBuckets()
    {
        this.logger.GetBuckets_LogEntering();

        List<HighLevelBucketInfo> result = new List<HighLevelBucketInfo>();
        foreach (BucketCreationData info in this.lowLevelStorage.ReadBucketsComplete())
        {
            result.Add(new HighLevelBucketInfo(info.Name,
                (string.IsNullOrEmpty(info.Description)) ? null : info.Description,
                DateTimeOffset.FromUnixTimeMilliseconds(info.CreateTimeUnixTimestampInMs)));
        }

        return result;
    }

    public List<string> GetMeasurements(string bucketName)
    {
        this.logger.GetMeasurements_LogEntering(bucketName);

        if (!this.lowLevelStorage.TryGetBucketId(bucketName, out uint bucketId))
        {
            this.logger.LogBucketDoesNotExists(bucketName);
            throw new YatsdbDataException($"Bucket {bucketName} does not exists.");
        }

        return this.lowLevelStorage.ReadMeasurements(bucketId);
    }

    public void Insert(ReadOnlyMemory<char> bucket, IEnumerable<HighLevelInputDataPoint> points)
    {
        this.logger.Insert_LogEntering(bucket);

        if (!this.lowLevelStorage.TryGetBucketId(bucket.Span, out uint bucketId))
        {
            this.logger.LogBucketDoesNotExists($"{bucket.Span}");
            throw new YatsdbDataException($"Bucket {bucket.Span} does not exists.");
        }

        Enumerable.TryGetNonEnumeratedCount(points, out int enumeratedCount);
        if (enumeratedCount == 1)
        {
            HighLevelInputDataPoint point = points.Single();

            if (!this.lowLevelStorage.TryEnsureMeasurementId(bucketId,
                    point.MeasurementName.Span,
                    out uint measurementId))
            {
                this.logger.Insert_LogErrorEnshuringMeasurementId($"{bucket.Span}", $"{point.MeasurementName.Span}");
                throw new YatsdbException($"Error by ensure measurementId.");
            }

            this.lowLevelStorage.InsertDataPoint(bucketId,
                measurementId,
                point.UnixTimeStampInMs,
                point.Values.Span,
                point.Tag.Span);
        }
        else
        {
            Dictionary<ReadOnlyMemory<char>, uint> measurementIds = new Dictionary<ReadOnlyMemory<char>, uint>(new MemStringEqualityComparer());

            foreach (HighLevelInputDataPoint point in points)
            {
                if (!measurementIds.TryGetValue(point.MeasurementName, out uint measurementId))
                {
                    if (!this.lowLevelStorage.TryEnsureMeasurementId(bucketId,
                        point.MeasurementName.Span,
                        out uint newMeasurementId))
                    {
                        this.logger.Insert_LogErrorEnshuringMeasurementId($"{bucket.Span}", $"{point.MeasurementName.Span}");
                        throw new YatsdbException($"Error by ensure measurementId.");
                    }

                    measurementIds.Add(point.MeasurementName, newMeasurementId);
                    measurementId = newMeasurementId;
                }

                this.lowLevelStorage.InsertDataPoint(bucketId,
                    measurementId,
                    point.UnixTimeStampInMs,
                    point.Values.Span,
                    point.Tag.Span);
            }
        }
    }

    public PreparedQueryObject PrepareQuery(QueryObject queryObject)
    {
        this.logger.PrepareQuery_LogEntering(queryObject.BucketName);

        if (!this.lowLevelStorage.TryGetBucketId(queryObject.BucketName.Span, out uint bucketId))
        {
            this.logger.LogBucketDoesNotExists($"{queryObject.BucketName.Span}");
            throw new YatsdbDataException($"Bucket {queryObject.BucketName.Span} does not exists.");
        }

        if (!this.lowLevelStorage.TryGetMeasurementId(bucketId, queryObject.MeasurementName.Span, out uint measurementId))
        {
            throw new YatsdbDataException($"Measurement {queryObject.MeasurementName.Span} does not exists.");
        }

        int[] valueIndexes = Array.Empty<int>();
        AggregationQuery[] aggregationInstances = Array.Empty<AggregationQuery>();

        if (queryObject.Aggregations != null)
        {
            if (queryObject.Aggregations.Any(t => t.AggregationType == AggregationType.Identity)
                && !queryObject.Aggregations.All(t => t.AggregationType == AggregationType.Identity))
            {
                throw new YatsdbSyntaxException("All columns must contains aggregation function in query.");
            }

            aggregationInstances = new AggregationQuery[queryObject.Aggregations.Count];
            valueIndexes = queryObject.Aggregations.Select(t => t.Index)
               .Distinct()
               .Order()
               .ToArray();

            for (int i = 0; i < queryObject.Aggregations.Count; i++)
            {
                aggregationInstances[i] = queryObject.Aggregations[i] with
                {
                    Index = Array.IndexOf(valueIndexes, queryObject.Aggregations[i].Index)
                };
            }
        }

        PreparedQueryObject preparedQueryObject = new PreparedQueryObject()
        {
            Type = PreparedQueryType.None,
            BucketNameId = bucketId,
            MeasurementId = measurementId,
            From = queryObject.From,
            To = queryObject.To,
            Skip = queryObject.Skip,
            Take = queryObject.Take,
            GroupByMs = queryObject.GroupByMs,
            Aggregations = aggregationInstances,
            RequestedTag = queryObject.RequestedTag,
            ValueIndexes = valueIndexes
        };

        if (queryObject.Aggregations != null)
        {
            if (queryObject.Aggregations.Any(t => t.AggregationType == AggregationType.Identity))
            {
                if (preparedQueryObject.GroupByMs.HasValue)
                {
                    throw new YatsdbSyntaxException("GroupByMs for identity function must by NULL.");
                }

                preparedQueryObject.GroupByMs = 1L;
                preparedQueryObject.Type = PreparedQueryType.WithGroupBy;
            }
            else
            {
                preparedQueryObject.Type = (preparedQueryObject.GroupByMs.HasValue)
                    ? PreparedQueryType.WithGroupBy
                    : PreparedQueryType.GroupAll;
            }
        }
        else
        {
            preparedQueryObject.Type = PreparedQueryType.StarExpression;
        }


        return preparedQueryObject;
    }

    public List<DbValue[]> ExecuteQuery(PreparedQueryObject preparedQueryObject)
    {
        System.Diagnostics.Debug.Assert(preparedQueryObject != null);
        System.Diagnostics.Debug.Assert(preparedQueryObject.Type != PreparedQueryType.None);

        this.logger.ExecuteQuery_LogEntering(preparedQueryObject.BucketNameId, preparedQueryObject.MeasurementId);

        int aggregationsCount = preparedQueryObject.ValueIndexes.Length;
        Span<double> queryValues = (aggregationsCount < 100) ? stackalloc double[aggregationsCount] : new double[aggregationsCount];

        AggregationInstance[] aggregationFns = new AggregationInstance[preparedQueryObject.Aggregations.Length];
        for (int i = 0; i < aggregationFns.Length; i++)
        {
            aggregationFns[i] = new AggregationInstance(preparedQueryObject.Aggregations[i].Index,
                AggregationFunctionFactory.Create(preparedQueryObject.Aggregations[i].AggregationType));
        }

        long? fromTime = preparedQueryObject.From.GetTimeValue(this.timeProvider);
        using LowLevelIterator iterator = this.lowLevelStorage.QueryData(preparedQueryObject.BucketNameId,
            preparedQueryObject.MeasurementId,
            fromTime,
            preparedQueryObject.To.GetTimeValue(this.timeProvider),
            preparedQueryObject.RequestedTag.Span);

        List<DbValue[]> response = default!;

        if (preparedQueryObject.Type == PreparedQueryType.WithGroupBy)
        {
            long startTime = fromTime ?? 0L;
            bool isFirst = true;
            long timespanMin = 0;
            long timespanMax = 0;
            long groupId = -1L;
            SkipTakeCollector collector = new SkipTakeCollector(preparedQueryObject.Skip, preparedQueryObject.Take);

            System.Diagnostics.Debug.Assert(preparedQueryObject.GroupByMs.HasValue);
            while (iterator.MoveNext(preparedQueryObject.ValueIndexes, out long timespan, queryValues))
            {
                long actualGroupId = (timespan - startTime) / preparedQueryObject.GroupByMs.Value;

                if (groupId != actualGroupId)
                {
                    groupId = actualGroupId;


                    if (!isFirst)
                    {
                        if (collector.CanAdd())
                        {
                            collector.Add(this.CollectValues(aggregationFns, timespanMin, timespanMax));
                            if (collector.CanBreak())
                            {
                                isFirst = true;
                                break;
                            }
                        }
                    }

                    this.ClearAggregations(aggregationFns);
                    isFirst = true;
                }

                if (isFirst)
                {
                    timespanMin = timespan;
                    isFirst = false;
                }

                timespanMax = timespan;

                for (int i = 0; i < aggregationFns.Length; i++)
                {
                    AggregationInstance aggregation = aggregationFns[i];
                    aggregation.Aggregation.Insert(timespan, queryValues[aggregation.Index]);
                }
            }

            if (!isFirst)
            {
                if (collector.CanAdd())
                {
                    collector.Add(this.CollectValues(aggregationFns, timespanMin, timespanMax));
                }
            }

            response = collector.IntoValues();
        }


        if (preparedQueryObject.Type == PreparedQueryType.GroupAll)
        {
            bool isFirst = true;
            long timespanMin = 0;
            long timespanMax = 0;
            response = new List<DbValue[]>();

            this.ClearAggregations(aggregationFns);

            while (iterator.MoveNext(preparedQueryObject.ValueIndexes, out long timespan, queryValues))
            {
                if (isFirst)
                {
                    timespanMin = timespan;
                    isFirst = false;
                }

                timespanMax = timespan;

                for (int i = 0; i < aggregationFns.Length; i++)
                {
                    AggregationInstance aggregation = aggregationFns[i];
                    aggregation.Aggregation.Insert(timespan, queryValues[aggregation.Index]);
                }
            }

            if (!isFirst)
            {
                response.Add(this.CollectValues(aggregationFns, timespanMin, timespanMax));
            }
        }

        if (preparedQueryObject.Type == PreparedQueryType.StarExpression)
        {
            SkipTakeCollector collector = new SkipTakeCollector(preparedQueryObject.Skip, preparedQueryObject.Take);

            PooledLowLevelStringAllocator stringAllocator = new PooledLowLevelStringAllocator();
            while (iterator.MoveNext(ref stringAllocator, out LowLevelDataPoint? point))
            {
                if (collector.CanAdd())
                {
                    collector.Add(this.CollectValues(point));
                    if (collector.CanBreak())
                    {
                        break;
                    }
                }
            }

            response = collector.IntoValues();
        }

        return response;
    }

    private void ClearAggregations(AggregationInstance[] aggregations)
    {
        for (int i = 0; i < aggregations.Length; i++)
        {
            aggregations[i].Aggregation.Reset();
        }
    }

    private DbValue[] CollectValues(AggregationInstance[] aggregations, long minTimespan, long maxTimespan)
    {
        DbValue[] values = new DbValue[aggregations.Length + 2];

        for (int i = 0; i < aggregations.Length; i++)
        {
            values[i] = aggregations[i].Aggregation.GetAggregated();
        }

        values[aggregations.Length] = DbValue.CreateFromLong(minTimespan, true);
        values[aggregations.Length + 1] = DbValue.CreateFromLong(maxTimespan, true);

        return values;
    }

    private DbValue[] CollectValues(LowLevelDataPoint dataPoint)
    {
        DbValue[] values = new DbValue[dataPoint.Values.Length + 2];
        for (int i = 0; i < dataPoint.Values.Length; i++)
        {
            values[i] = DbValue.CreateFromDouble(dataPoint.Values[i], true);
        }

        values[dataPoint.Values.Length] = DbValue.CreateFromLong(dataPoint.UnixTimestampInMs, true);
        values[dataPoint.Values.Length + 1] = DbValue.CreateFromString(dataPoint.Tag);

        return values;
    }
}
