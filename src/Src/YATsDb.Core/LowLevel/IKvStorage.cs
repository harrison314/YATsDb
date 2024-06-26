using System.Diagnostics.CodeAnalysis;

namespace YATsDb.Core.LowLevel;

public interface IKvStorage
{
    void Upsert(string key1, string key2, string value);

    void Upsert(IEnumerable<KvStorageEntity> entities);

    bool Remove(string key1, string key2);

    bool TryGet(string key1, string key2, [NotNullWhen(true)] out string? value);

    IEnumerable<string> EnumerateKeys(string key1);

    IEnumerable<KeyValuePair<string, string>> EnumerateKeyValues(string key1);
}
