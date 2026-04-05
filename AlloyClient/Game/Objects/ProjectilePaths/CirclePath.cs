#region

using System;
using OpenTK.Mathematics;
using AlloyClient.Networking;
using AlloyClient.Utils;

#endregion

namespace AlloyClient.Game.Objects.ProjectilePaths;

public class CirclePath : ProjectilePathSegment
{
    private float radius;

    public CirclePath() : base(PathType.CirclePath){}
    
    public CirclePath(float rotationsPerSecond, float radius, float? angle = null, int? lifetimeMs = null, int? timeOffset = null, params PathSegmentModifier[] mods)
        : base(PathType.CirclePath, rotationsPerSecond, angle, lifetimeMs, timeOffset, mods)
    {
        this.radius = radius;
    }

    public override Vector2 PositionAt(float elapsedLifetimeMs)
    {
        var p = Vector2.Zero;
        if (TimeOffset > 0 && elapsedLifetimeMs < TimeOffset)
            return p;

        elapsedLifetimeMs -= TimeOffset;

        ApplyModifiers(ref elapsedLifetimeMs);

        var elapsedSeconds = elapsedLifetimeMs / 1000f;
        float angle = 0;
        if (elapsedSeconds != 0)
            angle = Angle + (Speed * elapsedSeconds * 360f.Deg2Rad());

        p.X = MathF.Cos(angle) * radius;
        p.Y = MathF.Sin(angle) * radius;
        return p;
    }

    public override void Read(ref SpanReader rdr)
    {
        base.Read(ref rdr);
        radius = rdr.ReadSingle();
    }

    public override ProjectilePathSegment Clone()
    {
        return new CirclePath(Speed / 50, radius, _angle, _lifetimeMs, TimeOffset);
    }
}