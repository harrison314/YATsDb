using System.Diagnostics.CodeAnalysis;
using Tenray.ZoneTree;
using YATsDb.Core.TupleEncoding;

namespace YATsDb.Core.LowLevel;

public class KvStorage : IKvStorage
{
    private readonly IZoneTree<byte[], byte[]> zoneTree;

    public KvStorage(IZoneTree<byte[], byte[]> zoneTree)
    {
        this.zoneTree = zoneTree;
    }

    public IEnumerable<string> EnumerateKeys(string key1)
    {
        byte[] keyPrefix = TupleEncoder.Create(DataType.ApplicationDataPrefix, key1);

        IZoneTreeIterator<byte[], byte[]> iterator = this.zoneTree.CreateReverseIterator(IteratorType.AutoRefresh, includeDeletedRecords: false);

        iterator.Seek(in keyPrefix);

        while (iterator.Next())
        {
            if (!iterator.CurrentKey.AsSpan().StartsWith(keyPrefix))
            {
                break;
            }

            if (TupleEncoder.TryDeconstruct(iterator.CurrentKey,
                DataType.ApplicationDataPrefix,
                out string _,
                out string key2))
            {
                yield return key2;
            }
            else
            {
                //TODO: log
                break;
            }
        }
    }

    public IEnumerable<KeyValuePair<string, string>> EnumerateKeyValues(string key1)
    {
        byte[] keyPrefix = TupleEncoder.Create(DataType.ApplicationDataPrefix, key1);

        IZoneTreeIterator<byte[], byte[]> iterator = this.zoneTree.CreateReverseIterator(IteratorType.AutoRefresh, includeDeletedRecords: false);

        iterator.Seek(in keyPrefix);

        while (iterator.Next())
        {
            if (!iterator.CurrentKey.AsSpan().StartsWith(keyPrefix))
            {
                break;
            }

            if (TupleEncoder.TryDeconstruct(iterator.CurrentKey,
                DataType.ApplicationDataPrefix,
                out string _,
                out string key2)
                && TupleEncoder.TryDeconstruct(iterator.CurrentValue,
                DataType.ApplicationDataPrefix,
                out string value))
            {
                yield return new KeyValuePair<string, string>(key2, value);
            }
            else
            {
                //TODO: log
                break;
            }
        }
    }

    public bool Remove(string key1, string key2)
    {
        byte[] keyData = TupleEncoder.Create(DataType.ApplicationDataPrefix, key1, key2);
        return this.zoneTree.TryDelete(in keyData);
    }

    public void Upsert(string key1, string key2, string value)
    {
        byte[] keyData = TupleEncoder.Create(DataType.ApplicationDataPrefix, key1, key2);
        byte[] valueData = TupleEncoder.Create(DataType.ApplicationDataPrefix, value);

        // this.zoneTree.AtomicUpsert(in keyData, in valueData);
        this.zoneTree.Upsert(in keyData, in valueData);
    }

    public void Upsert(IEnumerable<KvStorageEntity> entities)
    {
        //TODO: transactional access
        foreach (KvStorageEntity entity in entities)
        {
            byte[] keyData = TupleEncoder.Create(DataType.ApplicationDataPrefix, entity.Key1, entity.Key2);
            byte[] valueData = TupleEncoder.Create(DataType.ApplicationDataPrefix, entity.Value);

            // this.zoneTree.AtomicUpsert(in keyData, in valueData);
            this.zoneTree.Upsert(in keyData, in valueData);
        }
    }

    public bool TryGet(string key1, string key2, [NotNullWhen(true)] out string? value)
    {
        byte[] keyData = TupleEncoder.Create(DataType.ApplicationDataPrefix, key1, key2);

        if (this.zoneTree.TryGet(in keyData, out byte[] valueData))
        {
            if (TupleEncoder.TryDeconstruct(valueData,
                DataType.ApplicationDataPrefix,
                out string stringValue))
            {

                value = stringValue;
                return true;
            }
            else
            {
                //TODO: log - bad encoding
            }
        }

        value = null;
        return false;
    }
}