using System;
using System.Runtime.InteropServices;
using AlloyClient.Utils;
using Alloy.Common;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace AlloyClient.Game;

public static class Camera {

    public const int HudOffset = 240;

    public static Vector2i Viewport;

    public static Matrix4 WorldMatrix;
    public static Matrix4 ViewMatrix;
    public static Matrix4 ProjectionMatrix;
    public static Matrix4 ZoomMatrix;
    public static Matrix4 BillboardMatrix;

    public static Vector2 VisibleTileRadius;

    public static float CameraAngle;

    public static Vector3 Position;

    static Camera() {
        Reset();
    }

    public static void Reset(bool includeHud = true) {
        CameraAngle = 0f;

        Position = new Vector3(0f, 0f, 12);

        WorldMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(180));
        ViewMatrix = Matrix4.Identity;
        BillboardMatrix = Matrix4.Identity;

        var halfWidth = Settings.ScreenWidth.Value;
        var halfHeight = Settings.ScreenHeight.Value;
        var hudOffset = includeHud ? HudOffset : 0f;

        Viewport = new Vector2i(halfWidth, halfHeight);
        ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(-halfWidth + hudOffset, halfWidth + hudOffset,
            -halfHeight, halfHeight, -10000f, 10000f);

        ZoomMatrix = Matrix4.CreateScale(Settings.CameraZoom);
    }

    public static void SetZoom(float zoom) {
        ZoomMatrix = Matrix4.CreateScale(zoom);
    }

    public static void UpdateViewPort(int width, int height) {
        Viewport = new Vector2i(width, height);
        
        GL.Viewport(0, 0, width, height);
        ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(-width + HudOffset, width + HudOffset, -height, height, -10000f, 10000f);
    }

    public static void Update(float x, float y) {
        CameraAngle = -Settings.CameraAngle;

        Position.X = x;
        Position.Y = -y;

        var s = MathF.Sin(CameraAngle);
        var c = MathF.Cos(CameraAngle);
        var s1 = MathF.Sin(CameraAngle - MathHelper.PiOver2);

        ViewMatrix = Matrix4.Identity;
        ViewMatrix.Row2 = new Vector4(s, s1, -1, 0);
        ViewMatrix.Row3 = new Vector4(-x, y, -12, 1);

        ViewMatrix *= Matrix4.CreateRotationZ(-CameraAngle);
        ViewMatrix *= Matrix4.CreateScale(100);
        ViewMatrix *= ZoomMatrix;

        //TODO: make bb mat a 2x2 instead of 4x4 
        BillboardMatrix[0, 0] = c;
        BillboardMatrix[0, 1] = -s;
        BillboardMatrix[1, 0] = s;
        BillboardMatrix[1, 1] = c;

        VisibleTileRadius = new Vector2((Settings.ScreenWidth - HudOffset) / (100.0f * Settings.CameraZoom), Settings.ScreenHeight / (100.0f * Settings.CameraZoom));
    }

    // Only tested on MapEditor
    public static Vector3 ScreenToWorld(Vector2 mouse) {
        var mat = Matrix4.Invert(WorldMatrix * ViewMatrix * ProjectionMatrix);

        var x = MathUtils.Map(mouse.X, 0, Viewport.X, -1, 1);
        var y = MathUtils.Map(mouse.Y, Viewport.Y, 0, -1, 1);
        
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

    //public static Vector3 WorldToScreen(Vector3 worldPosition, Viewport viewport) {
    //    return viewport.Project(worldPosition, ProjectionMatrix, ViewMatrix, WorldMatrix);
    //}
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly struct DepthMatrix(Matrix4 m) {
    public readonly float M12 = m.M12;
    public readonly float M22 = m.M22;
    public readonly float M32 = m.M32;
    public readonly float M42 = m.M42;
}