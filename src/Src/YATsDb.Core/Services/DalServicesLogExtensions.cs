using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.Services;

internal static partial class DalServicesLogExtensions
{
    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Trace,
       Message = "Entering to InsertLines with bucketName {bucketName}.")]
    public static partial void ToInsertLines_LogEntering(this ILogger<DalServices> logger, string bucketName);

    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Debug,
       Message = "Inserting {count} points into {bucketName}.")]
    public static partial void InsertLines_LogCountLines(this ILogger<DalServices> logger, int count, string bucketName);

    [LoggerMessage(
       EventId = 0,
       Level = LogLevel.Trace,
       Message = "Entering to Query with bucketName {bucketName}.")]
    public static partial void Query_LogEntering(this ILogger<DalServices> logger, string bucketName);

    [LoggerMessage(
      EventId = 0,
      Level = LogLevel.Debug,
      Message = "Executed query {query} in {elapsed}.")]
    public static partial void Query_LogExecuted(this ILogger<DalServices> logger, string query, TimeSpan elapsed);

}
