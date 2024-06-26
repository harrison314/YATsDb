namespace YATsDb.Core.Utils;

internal class ObjRef<T> where T : struct
{
    public T Value
    {
        get;
        set;
    }

    public ObjRef(T initValue)
    {
        this.Value = initValue;
    }
}
