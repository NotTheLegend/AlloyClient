using System;
using OpenTK.Mathematics;
using RealmClient.Game;
using RealmClient.Game.Objects;
using RealmClient.ParticleEffects.Particles;
using RealmClient.Rendering.VertexData;

namespace RealmClient.ParticleEffects;

public class HitEffect : ParticleEffect
{
    private const int Buffer = 10;
    private readonly HitParticle[] _data = new HitParticle[Buffer];
    private readonly ParticleData[] _particles = new ParticleData[Buffer];
    
    private readonly Entity _parent;

    private readonly Vector4 _color;
    private int _count;
    private double _lastUpdate = -1;

    private double _lifeTime = 2000;

    public HitEffect(Entity entity, uint color) {
        _parent = entity;
        _color = GetColor(color);
    }

    public override bool Update(double time, double dt)
    {
        var count = 1;
        var start = _count;
        
        for (var i = start; i < _count + count; i++)
        {
            if (_count == Buffer) break;

            var dx = (float)(Random.Shared.NextDouble() * 2 - 1);
            var dy = (float)(Random.Shared.NextDouble() * 2 - 1);

            _data[_count] = new HitParticle(dx, dy);
            _particles[_count] = new ParticleData(new Vector3(_parent.Position.X, _parent.Position.Y, 0.5f), _color);

            _count++;
        }
        
        for (var i = _count - 1; i >= 0; i--)
        {
            ref var data = ref _data[i];
            ref var particle = ref _particles[i];
            
            particle.Position.X += data.X * (float)(dt * 0.004);
            particle.Position.Y += data.Y * (float)(dt * 0.004);
            
            particle.Color = _color;
            
            _lifeTime -= dt;
            
            if (_lifeTime <= 0) {
                return false;
            }
        }
        
        Map.AddParticles(_particles, _count);
        _lastUpdate = time;

        return true;
    }
}