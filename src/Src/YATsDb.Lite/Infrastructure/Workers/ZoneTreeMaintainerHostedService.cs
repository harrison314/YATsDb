using Tenray.ZoneTree.Core;
using Tenray.ZoneTree;

namespace YATsDb.Lite.Infrastructure.Workers;

//https://github.com/koculu/ZoneTree/discussions/53
public sealed class ZoneTreeMaintainerHostedService<TKey, TValue> : IHostedService, IDisposable
{
    private readonly ZoneTreeMaintainer<TKey, TValue> zoneTreeMaintainer;

    public ZoneTreeMaintainerHostedService(IZoneTree<TKey, TValue> zoneTree)
    {
        this.zoneTreeMaintainer = new ZoneTreeMaintainer<TKey, TValue>(zoneTree, false);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.zoneTreeMaintainer.EnableJobForCleaningInactiveCaches = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.zoneTreeMaintainer.EnableJobForCleaningInactiveCaches = false;
        return this.zoneTreeMaintainer.WaitForBackgroundThreadsAsync();
    }

    public void Dispose()
    {
        this.zoneTreeMaintainer?.Dispose();
    }
}
