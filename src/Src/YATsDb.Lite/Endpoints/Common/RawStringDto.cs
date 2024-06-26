using System.Reflection;

namespace YATsDb.Lite.Endpoints.Common;

public class RawStringDto
{
    public string Value
    {
        get;
    }

    public RawStringDto(string value)
    {
        this.Value = value;
    }

    public static async ValueTask<RawStringDto?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        using StreamReader streamReader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8);
        string content = await streamReader.ReadToEndAsync(context.RequestAborted);

        return new RawStringDto(content);
    }
}