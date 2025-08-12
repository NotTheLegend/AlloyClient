using System;
using OpenTK.Mathematics;
using RealmClient.Objects;
using RealmClient.ParticleEffects.Particles;
using RealmClient.Rendering.VertexData;

namespace RealmClient.ParticleEffects;

public class FountainEffect : ParticleEffect {

    private const int Buffer = 50;

    private int _count;
    private readonly ParticleData[] _particles = new ParticleData[Buffer];
    private readonly FountainParticle[] _data = new FountainParticle[Buffer];

    private readonly Entity _parent;

    private readonly Vector4 _color;

    private double _lastUpdate = -1;

    public FountainEffect(Entity entity) {
        _parent = entity;
        _color = GetColor(0x4165D5);
    }
    
    public override bool Update(double time, double dt) {
        if (_lastUpdate < 0) _lastUpdate = Math.Max(0, time - 400);
        
        var count = (int)(time / 50);
        var start = (int)(_lastUpdate / 50);

        for (var i = start; i < count; i++) {
            if (_count == Buffer) break;
            
            _data[_count] = new FountainParticle(time);
            _particles[_count] = new ParticleData(SetStartPosition(_data[_count], time), _color);
            
            _count++;
        }

        for (var i = _count - 1; i >= 0; i--) {
            var data = _data[i];
            
            var t = (float) (time - data.StartTime) / 1000f;
            var z = 0.75f + FountainParticle.VI * t + FountainParticle.G * (t * t);

            if (z <= 0) {
                _particles[i] = _particles[_count - 1];
                _data[i] = _data[_count - 1];
                _count--;
                continue;
            }
            
            ref var part = ref _particles[i];
            part.Position.X += data.Velocity.X * (float) (dt * 0.0008);
            part.Position.Y += data.Velocity.Y * (float) (dt * 0.0008);
            part.Position.Z = z;
        }
        
        Map.AddParticles(_particles, _count);

        _lastUpdate = time;
        return true;
    }

    private Vector3 SetStartPosition(FountainParticle data, double time) {
        var dt = (float)(time - data.StartTime) * 0.0008f;
        var t = dt / 1000;
        var pos = new Vector3(_parent.Position, 0.75f + FountainParticle.VI * t + FountainParticle.G * (t * t));
        pos.X += data.Velocity.X * dt;
        pos.Y += data.Velocity.Y * dt;
        return pos;
    }
}