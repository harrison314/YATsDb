using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tenray.ZoneTree;
using Xunit.Abstractions;
using YATsDb.Core.LowLevel;

namespace YATsDb.Core.Tests.LowLevel;

public class KvStorageTests : IDisposable
{
    private string directory;
    private readonly ITestOutputHelper output;

    public KvStorageTests(ITestOutputHelper output)
    {
        this.directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.directory);
        this.output = output;
    }

    public void Dispose()
    {
        Directory.Delete(this.directory, true);
    }

    [Fact]
    public void KvStorage_Upsert()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));
        KvStorage kvStorage = new KvStorage(db);

        kvStorage.Upsert("key1", "aaa", "bbb");
        kvStorage.Upsert("key1", "cccc", "cccddd");
        kvStorage.Upsert("key1", "number:45", "12");
        kvStorage.Upsert("key1", "aaa", "bbb2");
    }

    [Fact]
    public void KvStorage_TryGet_Normal()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));
        KvStorage kvStorage = new KvStorage(db);

        kvStorage.Upsert("key1", "key2", "item1");

        bool result = kvStorage.TryGet("key1", "key2", out string? value);
        Assert.True(result);
        Assert.Equal("item1", value);
    }

    [Fact]
    public void KvStorage_TryGet_NonExists()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));
        KvStorage kvStorage = new KvStorage(db);

        kvStorage.Upsert("key1", "key2", "item1");

        bool result = kvStorage.TryGet("key1", "key2:v2", out string? value);
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void KvStorage_TryGet_Override()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));
        KvStorage kvStorage = new KvStorage(db);

        kvStorage.Upsert("key1", "key2", "item1");
        kvStorage.Upsert("key1", "key2", "item2");

        bool result = kvStorage.TryGet("key1", "key2", out string? value);
        Assert.True(result);
        Assert.Equal("item2", value);
    }

    [Fact]
    public void KvStorage_Remove_Normal()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));
        KvStorage kvStorage = new KvStorage(db);

        kvStorage.Upsert("key1", "key2", "item1");

        bool result = kvStorage.Remove("key1", "key2");
        Assert.True(result);

        bool result2 = kvStorage.TryGet("key1", "key2", out string? value);
        Assert.False(result2);
    }

    [Fact]
    public void KvStorage_Remove_Nonexists()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));
        KvStorage kvStorage = new KvStorage(db);

        kvStorage.Upsert("key1", "key2", "item1");

        bool result = kvStorage.Remove("key1", "key2:45");
        Assert.False(result);

        bool result2 = kvStorage.TryGet("key1", "key2:45", out string? value);
        Assert.False(result);
    }

    [Fact]
    public void KvStorage_EnumerateKeys_Normal()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));
        KvStorage kvStorage = new KvStorage(db);

        kvStorage.Upsert("key0", "0000", "bad value");
        kvStorage.Upsert("key2", "0000", "bad value");
        kvStorage.Upsert("key1:123", "0000", "bad value");

        kvStorage.Upsert("key1", "key2", "item1");
        kvStorage.Upsert("key1", "key2-a", "item1-a");
        kvStorage.Upsert("key1", "key2-b", "item1-b");

        string[] result = kvStorage.EnumerateKeys("key1").ToArray();

        Assert.Equal(3, result.Length);
        Assert.Contains("key2", result);
        Assert.Contains("key2-a", result);
        Assert.Contains("key2-b", result);
    }

    [Fact]
    public void KvStorage_EnumerateKeys_NonExists()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));
        KvStorage kvStorage = new KvStorage(db);

        kvStorage.Upsert("key0", "0000", "bad value");
        kvStorage.Upsert("key2", "0000", "bad value");
        kvStorage.Upsert("key1:123", "0000", "bad value");

        kvStorage.Upsert("key1:v3", "key2", "item1");
        kvStorage.Upsert("key1:v3", "key2-a", "item1-a");
        kvStorage.Upsert("key1:v3", "key2-b", "item1-b");

        string[] result = kvStorage.EnumerateKeys("key1").ToArray();

        Assert.Equal(0, result.Length);
    }

    [Fact]
    public void KvStorage_EnumerateKeyValues_Normal()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));
        KvStorage kvStorage = new KvStorage(db);

        kvStorage.Upsert("key0", "0000", "bad value");
        kvStorage.Upsert("key2", "0000", "bad value");
        kvStorage.Upsert("key1:123", "0000", "bad value");

        kvStorage.Upsert("key1", "key2", "item1");
        kvStorage.Upsert("key1", "key2-a", "item1-a");
        kvStorage.Upsert("key1", "key2-b", "item1-b");

        KeyValuePair<string, string>[] result = kvStorage.EnumerateKeyValues("key1").ToArray();

        Assert.Equal(3, result.Length);
        Assert.Contains("key2", result.Select(t => t.Key));
        Assert.Contains("key2-a", result.Select(t => t.Key));
        Assert.Contains("key2-b", result.Select(t => t.Key));

        Assert.Contains("item1", result.Select(t => t.Value));
        Assert.Contains("item1-a", result.Select(t => t.Value));
        Assert.Contains("item1-b", result.Select(t => t.Value));
    }

    [Fact]
    public void KvStorage_EnumerateKeyValues_NonExists()
    {
        using IZoneTree<Memory<byte>, Memory<byte>> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));
        KvStorage kvStorage = new KvStorage(db);

        kvStorage.Upsert("key0", "0000", "bad value");
        kvStorage.Upsert("key2", "0000", "bad value");
        kvStorage.Upsert("key1:123", "0000", "bad value");

        kvStorage.Upsert("key1:v3", "key2", "item1");
        kvStorage.Upsert("key1:v3", "key2-a", "item1-a");
        kvStorage.Upsert("key1:v3", "key2-b", "item1-b");

        KeyValuePair<string, string>[] result = kvStorage.EnumerateKeyValues("key1").ToArray();

        Assert.Equal(0, result.Length);
    }
}