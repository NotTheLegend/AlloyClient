using System;
using OpenTK.Mathematics;

namespace MonoClient.ParticleEffects.Particles;

public struct FountainParticle {

    public const float G = -4.9f;
    public const float VI = 6.5f;
    public const float ZI = 0.75f;

    public double StartTime;
    public Vector2 Velocity;

    public FountainParticle(double startTime) {
        StartTime = startTime;
        
        var angle = 2 * MathHelper.Pi * Random.Shared.NextSingle();
        Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }

}