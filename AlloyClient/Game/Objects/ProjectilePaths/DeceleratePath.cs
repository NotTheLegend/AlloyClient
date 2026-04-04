#region

using System;
using OpenTK.Mathematics;

#endregion

namespace AlloyClient.Game.Objects.ProjectilePaths;

public class DeceleratePath : ProjectilePathSegment
{
    public DeceleratePath() : base(PathType.DeceleratePath) { }
    
    public DeceleratePath(float speed, float? angle = null, int? lifetimeMs = null, int? timeOffset = null, params PathSegmentModifier[] mods)
        : base(PathType.DeceleratePath, speed, angle, lifetimeMs, timeOffset, mods)
    { }

    public override Vector2 PositionAt(float elapsedLifetimeMs)
    {
        var speed = Speed;
        var p = Vector2.Zero;
        if (TimeOffset > 0 && elapsedLifetimeMs < TimeOffset)
            return p;

        elapsedLifetimeMs -= TimeOffset;

        ApplyModifiers(ref elapsedLifetimeMs);

        speed *= 2 - (elapsedLifetimeMs / (LifetimeMs + 10f));
        var dist = elapsedLifetimeMs * (speed / 1000f);

        p.X = dist * MathF.Cos(Angle);
        p.Y = dist * MathF.Sin(Angle);
        return p;
    }

    public override ProjectilePathSegment Clone()
    {
        return new DeceleratePath(Speed, _angle, _lifetimeMs, TimeOffset);
    }
}