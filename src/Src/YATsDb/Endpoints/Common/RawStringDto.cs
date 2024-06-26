using System.Reflection;

namespace YATsDb.Endpoints.Common;

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
        //TODO: check content type a content encoding http headers
        using StreamReader streamReader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8);
        string content = await streamReader.ReadToEndAsync(context.RequestAborted);

        return new RawStringDto(content);
    }
}
