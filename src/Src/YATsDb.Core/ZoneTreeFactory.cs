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
    internal static readonly IRefComparer<Memory<byte>> RefComparer = new ByteArrayComparerAscending();

    public static IZoneTree<Memory<byte>, Memory<byte>> Build(Action<ZoneTreeFactory<Memory<byte>, Memory<byte>>> configure)
    {
        ZoneTreeFactory<Memory<byte>, Memory<byte>> factory = new ZoneTreeFactory<Memory<byte>, Memory<byte>>();
        //factory.DisableDeleteValueConfigurationValidation(false);
        factory.SetComparer(RefComparer);
        factory.SetKeySerializer(new ByteArraySerializer());
        factory.SetValueSerializer(new ByteArraySerializer());
        factory.SetIsDeletedDelegate(static (in Memory<byte> key, in Memory<byte> value) => value.Span[0] == DataType.RemovedValue);
        factory.SetMarkValueDeletedDelegate(static (ref Memory<byte> x) => x.Span[0] = DataType.RemovedValue);

        configure(factory);

        return factory.OpenOrCreate();
    }
}
