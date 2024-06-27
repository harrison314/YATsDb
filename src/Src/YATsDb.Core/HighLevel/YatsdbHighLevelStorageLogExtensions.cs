using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.HighLevel;

internal static partial class YatsdbHighLevelStorageLogExtensions
{
    [LoggerMessage(
   EventId = 0,
   Level = LogLevel.Error,
   Message = "Bucket bucketName {bucketName} does not exists.")]
    public static partial void LogBucketDoesNotExists(this ILogger<YatsdbHighLevelStorage> logger, string bucketName);

    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Trace,
       Message = "Entering to CreateBucket with bucketName {bucketName}.")]
    public static partial void CreateBucket_LogEntering(this ILogger<YatsdbHighLevelStorage> logger, string bucketName);


    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Error,
       Message = "Bucket {bucketName} alerady exists.")]
    public static partial void CreateBucket_LogBucketAlreadyExists(this ILogger<YatsdbHighLevelStorage> logger, string bucketName);

    [LoggerMessage(
      EventId = 0,
      Level = LogLevel.Information,
      Message = "Created a new bucket {bucketName}.")]
    public static partial void CreateBucket_LogCreateBucket(this ILogger<YatsdbHighLevelStorage> logger, string bucketName);

    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Trace,
       Message = "Entering to DeleteBucket with bucketName {bucketName}.")]
    public static partial void DeleteBucket_LogEntering(this ILogger<YatsdbHighLevelStorage> logger, string bucketName);

    [LoggerMessage(
      EventId = 0,
      Level = LogLevel.Information,
      Message = "Bucket {bucketName} has been removed.")]
    public static partial void DeleteBucket_LogBuckedRemoved(this ILogger<YatsdbHighLevelStorage> logger, string bucketName);

    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Trace,
       Message = "Entering to GetBuckets.")]
    public static partial void GetBuckets_LogEntering(this ILogger<YatsdbHighLevelStorage> logger);

    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Trace,
       Message = "Entering to GetMeasurements with bucketName {bucketName}.")]
    public static partial void GetMeasurements_LogEntering(this ILogger<YatsdbHighLevelStorage> logger, string bucketName);


    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Trace,
       Message = "Entering to Insert with bucketName {bucketName}.")]
    public static partial void Insert_LogEntering(this ILogger<YatsdbHighLevelStorage> logger, ReadOnlyMemory<char> bucketName);

    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Error,
       Message = "Error by ensure measurementId, bucket {bucketName}, measurement {measurement}.")]
    public static partial void Insert_LogErrorEnshuringMeasurementId(this ILogger<YatsdbHighLevelStorage> logger, string bucketName, string measurement);


    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Trace,
       Message = "Entering to PrepareQuery with bucketName {bucketName}.")]
    public static partial void PrepareQuery_LogEntering(this ILogger<YatsdbHighLevelStorage> logger, ReadOnlyMemory<char> bucketName);

    [LoggerMessage(
      EventId = 0,
      Level = LogLevel.Trace,
      Message = "Entering to ExecuteQuery with bucketId {bucketId}, measurementId {measurementId}.")]
    public static partial void ExecuteQuery_LogEntering(this ILogger<YatsdbHighLevelStorage> logger, uint bucketId, uint measurementId);
}
