using System.ComponentModel.DataAnnotations;

namespace YATsDb.Services.Configuration;

public class JsEngineSetup
{
    [Required]
    [Range(0L, long.MaxValue)]
    public long MemoryLimit
    {
        get;
        init;
    }

    [Required]
    public TimeSpan Timeout
    {
        get;
        init;
    }

    public string? ModuleBasePath
    {
        get;
        init;
    }

    [Required]
    [Range(0, int.MaxValue)]
    public int MaxStatements
    {
        get;
        init;
    }

    [Required]
    [Range(0, int.MaxValue)]
    public int LimitRecursion
    {
        get;
        init;
    }

    [Required]
    public JsEngineApiSetup Api
    {
        get;
        init;
    }

    public JsEngineSetup()
    {
        this.Api = new JsEngineApiSetup();
    }
}
