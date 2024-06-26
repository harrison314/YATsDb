namespace YATsDb.Services.Implementation.JsEngine;

public class EnvironmentProvider
{
    public EnvironmentProvider()
    {

    }

    public string? GetEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }
}
