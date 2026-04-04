#region

using System;
using OpenTK.Mathematics;
using AlloyClient.Networking;

#endregion

namespace AlloyClient.Game.Objects.ProjectilePaths;

public class AmplitudePath : ProjectilePathSegment
{
    private float amplitude;
    private float frequency;

    public AmplitudePath() : base(PathType.AmplitudePath) { }

    public AmplitudePath(float speed, float amplitude, float frequency, float? angle = null, int? lifetimeMs = null, int? timeOffset = null, params PathSegmentModifier[] mods)
        : base(PathType.AmplitudePath, speed, angle, lifetimeMs, timeOffset, mods)
    {
        this.amplitude = amplitude;
        this.frequency = frequency;
    }

    public override Vector2 PositionAt(float elapsedLifetimeMs)
    {
        var p = Vector2.Zero;
        if (TimeOffset > 0 && elapsedLifetimeMs < TimeOffset)
            return p;

        elapsedLifetimeMs -= TimeOffset;

        ApplyModifiers(ref elapsedLifetimeMs);

        var dist = elapsedLifetimeMs * (Speed / 1000f);
        p.X = dist * MathF.Cos(Angle);
        p.Y = dist * MathF.Sin(Angle);

        var phase = Info.ProjId % 2 == 0 ? 0 : MathF.PI;
        var deflection = amplitude * MathF.Sin(phase + (elapsedLifetimeMs / (float)LifetimeMs * frequency * 2 * MathF.PI));
        p.X = p.X + (deflection * MathF.Cos(Angle + (MathF.PI / 2)));
        p.Y = p.Y + (deflection * MathF.Sin(Angle + (MathF.PI / 2)));
        return p;
    }

    public override void Read(ref SpanReader rdr)
    {
        base.Read(ref rdr);
        amplitude = rdr.ReadSingle();
        frequency = rdr.ReadSingle();
    }

    public override ProjectilePathSegment Clone()
    {
        return new AmplitudePath(Speed, amplitude, frequency, _angle, _lifetimeMs, TimeOffset);
    }
}