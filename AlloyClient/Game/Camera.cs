using System;
using System.Runtime.InteropServices;
using AlloyClient.Utils;
using OpenTK.Mathematics;

namespace AlloyClient.Game;

public readonly struct Camera(Vector2 pos, Matrix4 matrix, Matrix4 billboard, Vector2 visibleTiles, int widthOffset) {
    public const float BaseCameraZoom = 100f;
    
    public readonly Vector2 Position = pos;
    public readonly int WidthOffset = widthOffset;
    public readonly Matrix4 Matrix = matrix;
    public readonly DepthMatrix DepthMatrix = new (in matrix);
    public readonly Matrix4 BillboardMatrix = billboard;
    public readonly Vector2 VisibleTileRadius = visibleTiles;
    
    public static Camera Update(Vector2 pos, Vector3i viewport, float cameraAngle, float cameraZoom) {
        var s = MathF.Sin(-cameraAngle);
        var c = MathF.Cos(-cameraAngle);
        var zoom = BaseCameraZoom * cameraZoom;
        
        var matrix = Matrix4.CreateRotationX(MathHelper.Pi); // world
        matrix *= new Matrix4(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(s, -c, -1, 0), new Vector4(-pos.X, pos.Y, -12, 1)) * CreateScaleWithRotationZ(cameraAngle, zoom); // view
        matrix *= Matrix4.CreateOrthographicOffCenter(-viewport.X + viewport.Z, viewport.X + viewport.Z, -viewport.Y, viewport.Y, -10000f, 10000f); // perspective
        
        var billboard = Matrix4.Identity;
        billboard[0, 0] = c;
        billboard[0, 1] = -s;
        billboard[1, 0] = s;
        billboard[1, 1] = c;
        
        var visibleTiles = new Vector2((viewport.X - viewport.Z) / zoom, viewport.Y / zoom);
        
        return new Camera(pos, matrix, billboard, visibleTiles, viewport.Z);
    }
    
    public Vector3 ScreenToWorld(Vector2 mouse, Vector2i viewport) {
        var mat = Matrix4.Invert(Matrix);

        var x = MathUtils.Map(mouse.X, 0, viewport.X, -1, 1);
        var y = MathUtils.Map(mouse.Y, viewport.Y, 0, -1, 1);
        
        var near = new Vector3(x, y, 0);
        var far = new Vector3(x, y, 1);
        
        near = Vector3.TransformPosition(near, mat);
        far = Vector3.TransformPosition(far, mat);

        var direction = far - near;
        direction.Normalize();

        var z = -near.Z / direction.Z; // Optional z value
        var pos = near + direction * z;
        return pos;
    }
    
    public Vector2i WorldToScreen(Vector2 position, Vector2i viewport) => WorldToScreen(new Vector3(position, 0), viewport);

    public Vector2i WorldToScreen(Vector3 position, Vector2i viewport) {
        var clipSpace = new Vector4(position, 1f) * Matrix;
        var x = (int)((clipSpace.X + 1f) * 0.5f * viewport.X);
        var y = (int)((1f - clipSpace.Y) * 0.5f * viewport.Y);
        return new Vector2i(x, y);
    }

    private static Matrix4 CreateScaleWithRotationZ(float angle, float scale) {
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);

        var result = Matrix4.Identity;
        result.Row0.X = cos * scale;
        result.Row0.Y = sin * scale;
        result.Row1.X = -sin * scale;
        result.Row1.Y = cos * scale;
        result.Row2.Z = scale;
        
        return result;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly struct DepthMatrix(in Matrix4 m) {
    public readonly float M12 = m.M12;
    public readonly float M22 = m.M22;
    public readonly float M32 = m.M32;
    public readonly float M42 = m.M42;
}