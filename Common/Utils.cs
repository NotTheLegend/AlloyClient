using System.Runtime.CompilerServices;
using Common.Structs;
using OpenTK.Mathematics;

namespace Common;

public static class Utils {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Clamp(this Vector2i vector, Vector2i min, Vector2i max, out Vector2i pos) {
        var change = false;
        pos = vector;

        if (vector.X < min.X || vector.X > max.X) {
            pos.X = Math.Clamp(vector.X, min.X, max.X);
            change = true;
        }
        
        if (vector.Y < min.Y || vector.Y > max.Y) {
            pos.Y = Math.Clamp(vector.Y, min.Y, max.Y);
            change = true;
        }
            
        return change;
    }

    public static (int, int) ToPair(this Vector2i vector) => (vector.X, vector.Y);
    
    public static AtlasData ReadAtlasData(this BinaryReader reader) {
        return new AtlasData {
            U = reader.ReadSingle(),
            V = reader.ReadSingle(),
            W = reader.ReadSingle(),
            H = reader.ReadSingle()
        };
    }
}