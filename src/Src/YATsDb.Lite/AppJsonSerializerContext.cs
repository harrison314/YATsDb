using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YATsDb.Lite;

[JsonSerializable(typeof(Endpoints.ManagementGetBucketsEndpoint.BucketInfoDto))]
[JsonSerializable(typeof(List<Endpoints.ManagementGetBucketsEndpoint.BucketInfoDto>))]
[JsonSerializable(typeof(Endpoints.ManagementPostBucketsEndpoint.CreateBucketDto))]
[JsonSerializable(typeof(Endpoints.PostQueryEndpoint.QueryDal))]
[JsonSerializable(typeof(YATsDb.Lite.Endpoints.Common.QueryResult))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
