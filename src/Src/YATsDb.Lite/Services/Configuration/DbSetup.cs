using System.ComponentModel.DataAnnotations;

namespace YATsDb.Lite.Services.Configuration;

public partial class DbSetup
{
    [Required]
    public string DbPath
    {
        get;
        set;
    }

    [Required]
    public Tenray.ZoneTree.Options.WriteAheadLogMode WriteAheadLogMode
    {
        get;
        set;
    }

    public int? MaxMuttableSegmetsCount
    {
        get;
        set;
    }

    public DbSetup()
    {
        this.DbPath = string.Empty;
    }
}
