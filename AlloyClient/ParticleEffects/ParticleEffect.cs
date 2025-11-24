using AlloyClient.Game.Objects;
using OpenTK.Mathematics;

namespace AlloyClient.ParticleEffects;

public abstract class ParticleEffect {

    protected Vector2 _position;

    public void SetEntityPosition(Vector2 pos) => _position = pos;

    public abstract bool Update(double time, double dt);

    protected Vector4 GetColor(uint rgb) {
        var r = (byte)(rgb >> 16);
        var g = (byte)(rgb >> 8);
        var b = (byte)rgb;
        return new Vector4(r / 255f, g / 255f, b / 255f, -1);
    }

    public static ParticleEffect FromProperties(string effectName, Entity entity) {
        return effectName switch {
            "Fountain" => new FountainEffect(),
            _ => null
        };
    }

}