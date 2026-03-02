using AlloyClient.Game;
using AlloyClient.Rendering.VertexData;
using OpenTK.Mathematics;

namespace AlloyClient.ParticleEffects;

public class SparkEffect : ParticleEffect {
    private ParticleData _particle;

    private float _startingSize;
    private double _lifeTime;
    private double _timeLeft;

    private float _dx;
    private float _dy;

    public SparkEffect(int size, uint color, int lifetime, float z, Vector2 start, Vector2 end) {
        _startingSize = size;
        _lifeTime = _timeLeft = lifetime;
        _dx = (end.X - start.X) / (lifetime / 1000f);
        _dy = (end.Y - start.Y) / (lifetime / 1000f);
        _particle = new ParticleData(new Vector3(start, z), GetColor(color));
    }

    public SparkEffect(int size, uint color, int lifetime, float z, float dx, float dy, float x, float y) {
        _startingSize = size;
        _lifeTime = _timeLeft = lifetime;
        _dx = dx;
        _dy = dy;
        _particle = new ParticleData(new Vector3(x, y, z), GetColor(color));
    }

    public override bool Update(double time, double dt) {
        _timeLeft -= dt;
        if (_timeLeft <= 0) return false;

        var delta = (float)(dt / 1000.0);
        _particle.Position.X += _dx * delta;
        _particle.Position.Y += _dy * delta;
        _particle.Position.W = (float)(_timeLeft / _lifeTime * _startingSize / 100d);

        Map.AddParticle(_particle);
        return true;
    }
}