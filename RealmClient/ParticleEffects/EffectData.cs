using System;
using OpenTK.Mathematics;

namespace RealmClient.ParticleEffects;

public readonly struct FountainParticle(double startTime, float angle) {

    public const float G = -4.9f;
    public const float VI = 6.5f;
    public const float ZI = 0.75f;

    public readonly double StartTime = startTime;
    public readonly Vector2 Velocity = new(MathF.Cos(angle), MathF.Sin(angle));
}

public readonly struct HitParticle(float x, float y) {
    public readonly float X = x;
    public readonly float Y = y;
}