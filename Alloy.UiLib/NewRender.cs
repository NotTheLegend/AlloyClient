using System.Collections.Generic;

namespace Alloy.UiLib;

public static class NewRender {

    internal static readonly SpriteStatePool StatePool = new();


}






internal sealed class SpriteStatePool {
        
    private readonly List<int> _pool = [];

    private int _id;

    public int Pop() {
        if (_pool.Count < 1) {
            return _id++;
        }
            
        var list = _pool[^1];
        _pool.RemoveAt(_pool.Count - 1);
        return list;
    }

    public void Push(int id) {
        _pool.Add(id);
    }
}