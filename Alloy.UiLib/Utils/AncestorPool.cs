using System.Collections.Generic;
using Alloy.UiLib.Core;

namespace Alloy.UiLib.Utils;

internal sealed class AncestorPool(int initialCapacity = 16) {
        
    private readonly List<List<DisplayObject>> _pool = [with(initialCapacity)];

    public List<DisplayObject> Pop() {
        if (_pool.Count < 1) {
            return [];
        }
            
        var list = _pool[^1];
        _pool.RemoveAt(_pool.Count - 1);
        return list;
    }

    public void Push(List<DisplayObject> list) {
        list.Clear();
        _pool.Add(list);
    }
}