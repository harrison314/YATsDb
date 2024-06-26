namespace YATsDb.Core.Utils;

internal class LockFreeRingBuffer<T> where T : class
{
    private readonly T[] buffer;
    private readonly int capacity;
    private int head;
    private int tail;

    public LockFreeRingBuffer(int capacity)
    {
        this.capacity = capacity;
        this.buffer = new T[this.capacity];
        this.head = 0;
        this.tail = 0;
    }

    public bool TryWrite(T value)
    {
        do
        {
            int currentTail = this.tail;
            int nextTail = (currentTail + 1) % this.capacity;

            // Check if the buffer is full
            if (nextTail == Volatile.Read(ref this.head))
            {
                return false;
            }

            // Attempt to update the _tail index atomically
            if (Interlocked.CompareExchange(ref this.tail, nextTail, currentTail) == currentTail)
            {
                this.buffer[currentTail] = value;
                return true;
            }
        }
        while (true);
    }

    public bool TryRead(out T? value)
    {
        do
        {
            int currentHead = this.head;
            if (currentHead == Volatile.Read(ref this.tail))
            {
                value = default;
                return false;
            }

            // Attempt to update the _head index atomically
            T item = this.buffer[currentHead];
            if (Interlocked.CompareExchange(ref this.head, (currentHead + 1) % this.capacity, currentHead) == currentHead)
            {
                value = item;
                return true;
            }
        }
        while (true);
    }
}
