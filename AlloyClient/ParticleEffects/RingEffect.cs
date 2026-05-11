using AlloyClient.Game;
using AlloyClient.Game.Objects;
using OpenTK.Mathematics;
using System;

namespace AlloyClient.ParticleEffects;

internal class RingEffect : ParticleEffect {
    
    private const int SIZE = 100;
    private const int NUMPOINTS = 12;
    private const int LIFETIME = 200;
    
    public float Radius;
    public uint Color;

    public RingEffect(Entity entity, float radius, uint color) {
        _position = entity.Position;
        Radius = radius;
        Color = color;
    }

    public override bool Update(double time, double dt) {
        for (int i = 0; i < NUMPOINTS; i++) {
            float angle = i * 2 * MathF.PI / NUMPOINTS;
            var p = new Vector2(_position.X + Radius * MathF.Cos(angle), _position.Y + Radius * MathF.Sin(angle));
            var q = new Vector2(_position.X + Radius * 0.9f * MathF.Cos(angle),
                _position.Y + Radius * 0.9f * MathF.Sin(angle));
            var part = new SparkerEffect(SIZE, Color, LIFETIME, 0.5f, q, p);
            Map.AddParticleEffect(part);
        }

        return false;
    }
}