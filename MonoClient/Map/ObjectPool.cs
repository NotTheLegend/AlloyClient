using System.Collections.Generic;

namespace MonoClient;

public sealed class ObjectPool<T>(int capacity = 1000) where T : new() {

    private readonly List<T> pool = [];

    private int count;

    public int GetCount() => count;

    public T Pop() {
        if (count < 1)
            return new T();

        var obj = pool[0];
        pool.RemoveAt(0);
        count--;
        return obj;
    }

    public void Push(T obj) {
        if (count >= capacity)
            return;
        
        pool.Add(obj);
        count++;
    }
}