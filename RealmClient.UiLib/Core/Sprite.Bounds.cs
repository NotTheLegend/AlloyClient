using System;
using Common;
using RealmClient.UiLib.Enums;
using OpenTK.Mathematics;

namespace RealmClient.UiLib.Core;

public partial class Sprite {

    public bool IsInBounds(Vector2i pos) {
        if (pos.X < _scissor.X || pos.X > _scissor.Z || pos.Y < _scissor.Y || pos.Y > _scissor.W)
            return false;
        
        return HitboxType switch {
            HitboxType.Default => SquareHitbox(pos),
            HitboxType.Ellipse => EllipseHitbox(pos),
            HitboxType.Complex => ComplexHitbox(new Vector2i(pos.X - _trueX, pos.Y - _trueY)),
            HitboxType.Custom => CustomHitbox(new Vector2i(pos.X - _trueX, pos.Y - _trueY)),
            _ => throw new ArgumentOutOfRangeException($"{HitboxType} not handled in InternalBoundsCheck")
        };
    }

    private bool SquareHitbox(Vector2i pos) {
        var scale = _trueScale / Scale;// W/H already account for scale
        return pos.X >= _trueX && pos.X <= _trueX + _width * scale.X && pos.Y >= _trueY && pos.Y <= _trueY + _height * scale.Y;
    }
    
    private bool EllipseHitbox(Vector2i pos) {
        var (dx, dy) = Radii.ToPair();
        dx = (int) (dx * _trueScale.X);
        dy = (int) (dy * _trueScale.X);

        var (x, y) = (pos.X, pos.Y); // Mouse
        var (cx, cy) = (dx + _trueX, dy + _trueY); // Center
        var (rx, ry) = (Radii.X * _trueScale.X, Radii.Y * _trueScale.Y);
        
        return (x - cx) * (x - cx) / (rx * rx) + (y - cy) * (y - cy) / (ry * ry) <= 1;
    }
    
    private bool ComplexHitbox(Vector2i pos) {
        var len = Indices.Length;

        for (var i = 0; i < len; i += 3) {
            var t1 = VertexData[Indices[i + 0]].Position;
            var t2 = VertexData[Indices[i + 1]].Position;
            var t3 = VertexData[Indices[i + 2]].Position;

            var d1 = (pos.X - t2.X) * (t1.Y - t2.Y) - (t1.X - t2.X) * (pos.Y - t2.Y);
            var d2 = (pos.X - t3.X) * (t2.Y - t3.Y) - (t2.X - t3.X) * (pos.Y - t3.Y);
            var d3 = (pos.X - t1.X) * (t3.Y - t1.Y) - (t3.X - t1.X) * (pos.Y - t1.Y);
            
            if (!((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0)))
                return true;

            var count = _children.Count;
            for (var j = 0; j < count; j++) {
                _children[j].ComplexHitbox(pos);
            }
        }
        
        return false;
    }
    
    /// <param name="pos">mouse position local to sprite</param>
    protected virtual bool CustomHitbox(Vector2i pos) {
        throw new MissingMethodException("Sprite must define override for CustomHitbox");
    }
}