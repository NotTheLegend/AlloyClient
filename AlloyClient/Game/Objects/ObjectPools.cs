using System.Collections.Generic;
namespace AlloyClient.Game.Objects;

public static class ObjectPools {
    public readonly static ObjectPool<Projectile> Projectiles = new();
}

// any object you want to pool needs to impliment this interface :p
public interface IResettable {
    void Reset();
    bool IsInPool { get; set; }
}

public sealed class ObjectPool<T>(int initialCapacity = 1000) where T : IResettable, new() {
    private readonly List<T> _pool = new(initialCapacity);

    public T Pop() {
        if (_pool.Count < 1) return new T();
        var obj = _pool[^1];
        _pool.RemoveAt(_pool.Count - 1);
        obj.IsInPool = false;
        return obj;
    }

    public void Push(T obj) {
        if (obj.IsInPool) return;
        obj.Reset();
        obj.IsInPool = true;
        _pool.Add(obj);
    }

    public int Count => _pool.Count;
}