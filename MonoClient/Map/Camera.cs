using System;
using System.Runtime.InteropServices;
using MonoClient.State;
using OpenTK.Mathematics;

namespace MonoClient;

public static class Camera {

    public const int HudOffset = 240;

    public static Matrix4 WorldMatrix;
    public static Matrix4 ViewMatrix;
    public static Matrix4 ProjectionMatrix;
    public static Matrix4 ZoomMatrix;
    public static Matrix4 BillboardMatrix;

    public static Vector2 VisibleTileRadius;

    public static float CameraAngle;

    public static Vector3 Position;
    private static Vector3 _lookAt;

    static Camera() {
        Reset();
    }

    public static void Reset(bool includeHud = true) {
        CameraAngle = 0f;

        Position = new Vector3(0f, 0f, 12);
        _lookAt = new Vector3(0f);

        WorldMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(180));
        ViewMatrix = new Matrix4();
        BillboardMatrix = Matrix4.Identity;

        var halfWidth = Settings.DefaultScreenWidth;
        var halfHeight = Settings.DefaultScreenHeight;
        var hudOffset = includeHud ? HudOffset : 0f;

        ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(-halfWidth + hudOffset, halfWidth + hudOffset,
            -halfHeight, halfHeight, -10000f, 10000f);

        ZoomMatrix = Matrix4.CreateScale(Settings.CameraZoom);
    }

    public static void SetZoom(float zoom) {
        ZoomMatrix = Matrix4.CreateScale(zoom);
    }

    public static void Update(float x, float y) {
        CameraAngle = -Settings.CameraAngle;

        Position.X = x;
        Position.Y = -y;
        _lookAt.X = x;
        _lookAt.Y = -y;

        var s = MathF.Sin(CameraAngle);
        var c = MathF.Cos(CameraAngle);
        var s1 = MathF.Sin(CameraAngle - MathHelper.PiOver2);

        ViewMatrix = CreateLookAt(Position, _lookAt, new Vector3(0f, 1f, 0f));
        ViewMatrix[0, 2] = s;
        ViewMatrix[1, 2] = s1;
        ViewMatrix[2, 2] = -1f;
        ViewMatrix *= Matrix4.CreateRotationZ(-CameraAngle);
        ViewMatrix *= ZoomMatrix;

        BillboardMatrix[0, 0] = c;
        BillboardMatrix[1, 0] = -s;
        BillboardMatrix[0, 1] = s;
        BillboardMatrix[1, 1] = c;

        VisibleTileRadius = new Vector2((Settings.ScreenWidth - HudOffset) / Settings.CameraZoom, Settings.ScreenHeight / Settings.CameraZoom);
    }
    
    private static Matrix4 CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector) {
        var result = new Matrix4();
        var vector = Vector3.Normalize(cameraPosition - cameraTarget);
        var vector2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector));
        var vector3 = Vector3.Cross(vector, vector2);
        result.M11 = vector2.X;
        result.M12 = vector3.X;
        result.M13 = vector.X;
        result.M14 = 0f;
        result.M21 = vector2.Y;
        result.M22 = vector3.Y;
        result.M23 = vector.Y;
        result.M24 = 0f;
        result.M31 = vector2.Z;
        result.M32 = vector3.Z;
        result.M33 = vector.Z;
        result.M34 = 0f;
        result.M41 = -Vector3.Dot(vector2, cameraPosition);
        result.M42 = -Vector3.Dot(vector3, cameraPosition);
        result.M43 = -Vector3.Dot(vector, cameraPosition);
        result.M44 = 1f;
        return result;
    }

    // Only tested on MapEditor
    public static Vector3 ScreenToWorld(Vector2 mousePosition, Viewport viewport) {
        var near = new Vector3(mousePosition, 0);
        var far = new Vector3(mousePosition, 1);
        near = viewport.Unproject(near, ProjectionMatrix, ViewMatrix, WorldMatrix);
        far = viewport.Unproject(far, ProjectionMatrix, ViewMatrix, WorldMatrix);

        var direction = far - near;
        direction.Normalize();

        var z = -near.Z / direction.Z; // Optional z value
        var pos = near + direction * z;
        return pos;
    }

    public static Vector3 WorldToScreen(Vector3 worldPosition, Viewport viewport) {
        return viewport.Project(worldPosition, ProjectionMatrix, ViewMatrix, WorldMatrix);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly struct DepthMatrix(Matrix4 m) {
    public readonly float M12 = m.M12;
    public readonly float M22 = m.M22;
    public readonly float M32 = m.M32;
    public readonly float M42 = m.M42;
}