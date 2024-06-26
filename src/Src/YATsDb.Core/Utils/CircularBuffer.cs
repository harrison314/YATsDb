using System.Collections;
using System.Runtime.CompilerServices;

namespace YATsDb.Core.Utils;

public class CircularBuffer<T> : ICollection<T>
{
    private T[] buffer;
    private int start;
    private int end;

    public int Count
    {
        get => (this.end - this.start + this.buffer.Length) % this.buffer.Length;
    }

    public int Capacity
    {
        get => this.buffer.Length - 1;
    }

    public bool IsReadOnly
    {
        get => false;
    }

    public CircularBuffer(int capacity)
    {
        System.Diagnostics.Debug.Assert(capacity > 0);

        this.buffer = new T[capacity + 1];
        this.start = 0;
        this.end = 0;
    }

    public void Add(T item)
    {
        this.buffer[this.end] = item;
        this.end = this.IncrementIndex(this.end);
        if (this.end == this.start)
        {
            this.start = this.IncrementIndex(this.start);
        }
    }

    public void Clear()
    {
        this.start = 0;
        this.end = 0;

        if (!typeof(T).IsPrimitive)
        {
            Array.Fill<T>(this.buffer, default(T)!);
        }
    }

    public bool Contains(T item)
    {
        return this.Contains(item, EqualityComparer<T>.Default);
    }

    public bool Contains(T item, IEqualityComparer<T> comparer)
    {
        int bufferIndex = this.start;
        while (bufferIndex != this.end)
        {
            if (comparer.Equals(this.buffer[bufferIndex], item))
            {
                return true;
            }

            bufferIndex = this.IncrementIndex(bufferIndex);
        }
        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        int bufferIndex = this.start;
        while (bufferIndex != this.end)
        {
            array[arrayIndex++] = this.buffer[bufferIndex];
            bufferIndex = this.IncrementIndex(bufferIndex);
        }
    }

    public bool Remove(T item)
    {
        throw new NotSupportedException($"Renove is not supported in {this.GetType().FullName}");
    }

    public IEnumerator<T> GetEnumerator()
    {
        int bufferIndex = this.start;
        while (bufferIndex != this.end)
        {
            yield return this.buffer[bufferIndex];
            bufferIndex = this.IncrementIndex(bufferIndex);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public List<T> ToList()
    {
        List<T> list = new List<T>(this.Count);
        int bufferIndex = this.start;
        while (bufferIndex != this.end)
        {
            list.Add(this.buffer[bufferIndex]);
            bufferIndex = this.IncrementIndex(bufferIndex);
        }

        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int IncrementIndex(int index)
    {
        index++;
        return (index >= this.buffer.Length) ? index - this.buffer.Length : index;
    }
}
