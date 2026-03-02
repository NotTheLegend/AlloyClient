using AlloyClient.Game;
using AlloyClient.Rendering.VertexData;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AlloyClient.ParticleEffects;

internal class SparkerEffect : ParticleEffect {
    private int _startingSize;
    private int _lifeTime;
    private double _timeLeft;
    private uint _color;

    private float _dx;
    private float _dy;

    public SparkerEffect(int size, uint color, int lifetime, float z, Vector2 start, Vector2 end) {
        _position = start;
        _startingSize = size;
        _color = color;
        _timeLeft = _lifeTime = lifetime;
        _dx = (end.X - start.X) / (lifetime / 1000f);
        _dy = (end.Y - start.Y) / (lifetime / 1000f);
    }

    public override bool Update(double time, double dt) {
        _timeLeft -= dt;
        if (_timeLeft <= 0) return false;

        var delta = (float)(dt / 1000.0);
        _position += delta * new Vector2(_dx, _dy);
        Map.AddParticleEffect(new SparkEffect(100, _color, 600, 0.5f, _position,
            _position + new Vector2(Random.Shared.NextSingle() * 2 - 1, Random.Shared.NextSingle() * 2 - 1)));

        return true;
    }
}