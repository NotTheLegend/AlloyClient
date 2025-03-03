using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoClient.State;
using MonoClient.UiLib;

namespace MonoClient;

public static class Camera {

    public const int HudOffset = 240;
    
    public static Matrix WorldMatrix;
    public static Matrix ViewMatrix;
    public static Matrix ProjectionMatrix;
    public static Matrix ZoomMatrix;
    public static Matrix BillboardMatrix;

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

        WorldMatrix = Matrix.CreateRotationX(MathHelper.ToRadians(180));
        ViewMatrix = new Matrix();
        BillboardMatrix = Matrix.Identity;

        var halfWidth = Settings.DefaultScreenWidth;
        var halfHeight = Settings.DefaultScreenHeight;
        var hudOffset = includeHud ? HudOffset : 0f;

        ProjectionMatrix = Matrix.CreateOrthographicOffCenter(-halfWidth + hudOffset, halfWidth + hudOffset,
            -halfHeight, halfHeight, -10000f, 10000f);

        ZoomMatrix = Matrix.CreateScale(Settings.CameraZoom);
    }

    public static void SetZoom(float zoom) {
        ZoomMatrix = Matrix.CreateScale(zoom);
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

        ViewMatrix = Matrix.CreateLookAt(Position, _lookAt, new Vector3(0f, 1f, 0f));
        ViewMatrix[8] = s;
        ViewMatrix[9] = s1;
        ViewMatrix[10] = -1f;
        ViewMatrix *= Matrix.CreateRotationZ(-CameraAngle);
        ViewMatrix *= ZoomMatrix;

        BillboardMatrix[0] = c;
        BillboardMatrix[1] = -s;
        BillboardMatrix[4] = s;
        BillboardMatrix[5] = c;

        VisibleTileRadius = new Vector2((Settings.ScreenWidth - HudOffset) / Settings.CameraZoom, Settings.ScreenHeight / Settings.CameraZoom);
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