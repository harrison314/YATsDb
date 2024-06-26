using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tenray.ZoneTree.Comparers;
using Tenray.ZoneTree.Serializers;
using Tenray.ZoneTree;
using YATsDb.Core.TupleEncoding;

namespace YATsDb.Core;


public static class ZoneTreeFactory
{
    internal static readonly IRefComparer<byte[]> RefComparer = new ByteArrayComparerAscending();

    public static IZoneTree<byte[], byte[]> Build(Action<ZoneTreeFactory<byte[], byte[]>> configure)
    {
        ZoneTreeFactory<byte[], byte[]> factory = new ZoneTreeFactory<byte[], byte[]>();
        factory.DisableDeleteValueConfigurationValidation(false);
        factory.SetComparer(RefComparer);
        factory.SetKeySerializer(new ByteArraySerializer());
        factory.SetValueSerializer(new ByteArraySerializer());
        factory.SetIsValueDeletedDelegate(static (in byte[] x) => x[0] == DataType.RemovedValue);
        factory.SetMarkValueDeletedDelegate(static (ref byte[] x) => x[0] = DataType.RemovedValue);

        configure(factory);

        return factory.OpenOrCreate();
    }
}
