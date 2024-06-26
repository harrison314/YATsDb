using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.Json;
using YATsDb.Core.HighLevel;

namespace YATsDb.Lite.Endpoints.Common;

public class QueryResult
{
    [JsonConverter(typeof(ObjConvertor))]
    public List<object?[]> Result { get; }

    public QueryResult(List<object?[]> result)
    {
        this.Result = result;
    }

    private QueryResult()
    {
        this.Result = default!;
    }
}


internal class ObjConvertor : JsonConverter<List<object?[]>>
{
    public override bool HandleNull
    {
        get => true;
    }

    public override List<object?[]>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, List<object?[]> value, JsonSerializerOptions options)
    {

        writer.WriteStartArray();

        foreach (object?[] row in value)
        {
            writer.WriteStartArray();
            for (int i = 0; i < row.Length; i++)
            {
                this.WriteValue(writer, row[i]);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndArray();
    }

    private void WriteValue(Utf8JsonWriter writer, object? value)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        if (value is string str)
        {
            writer.WriteStringValue(str);
            return;
        }

        if (value is long longValue)
        {
            writer.WriteNumberValue(longValue);
            return;
        }

        if (value is double doubleValue)
        {
            writer.WriteNumberValue(doubleValue);
            return;
        }

        if (value is DateTimeOffset dateTimeOffsetValue)
        {
            writer.WriteStringValue(dateTimeOffsetValue);
            return;
        }

        throw new InvalidProgramException();
    }
}