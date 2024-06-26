using System.Runtime.CompilerServices;
using YATsDb.Core.Utils;

namespace YATsDb.Core.HighLevel;

internal struct SkipTakeCollector
{
    private int skip;
    private int take;
    private ICollection<DbValue[]> dbValues;

    public SkipTakeCollector(int skip, int? take)
    {
        this.skip = skip;

        if (take.HasValue)
        {
            if (take.Value < 0)
            {
                this.take = int.MaxValue;
                this.dbValues = new CircularBuffer<DbValue[]>(-take.Value);
            }
            else
            {
                this.take = take.Value;
                this.dbValues = new List<DbValue[]>(Math.Min(take.Value, 250));
            }
        }
        else
        {
            this.take = int.MaxValue;
            this.dbValues = new List<DbValue[]>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanAdd()
    {
        this.skip--;
        return this.skip <= 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(DbValue[] values)
    {
        this.dbValues.Add(values);
        this.take--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanBreak()
    {
        return this.take <= 0;
    }

    public List<DbValue[]> IntoValues()
    {
        return this.dbValues switch
        {
            List<DbValue[]> list => list,
            CircularBuffer<DbValue[]> cb => cb.ToList(),
            _ => throw new InvalidProgramException()
        };
    }
}
