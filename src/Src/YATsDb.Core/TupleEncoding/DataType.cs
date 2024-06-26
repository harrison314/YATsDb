namespace YATsDb.Core.TupleEncoding;

internal static class DataType
{
    public const byte RemovedValue = 0x00;

    public const byte KeyTsDataPrefix = 0x01;

    public const byte KeyTsDataIndexPrefix = 0x02;

    public const byte KeyHiloPrefix = 0x03;

    public const byte KeyBucketPrefix = 0x04;

    public const byte KeyMeasurementPrefix = 0x05;

    public const byte ApplicationDataPrefix = 0x06;


    public const byte ApplicationQueue = 0x07;
}
