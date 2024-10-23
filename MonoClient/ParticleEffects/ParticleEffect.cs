using Microsoft.Xna.Framework;

namespace MonoClient.ParticleEffects;

public abstract class ParticleEffect {

    public abstract bool Update(double time, double dt);

    protected Vector4 GetColor(uint rgb) {
        var r = (byte)(rgb >> 16);
        var g = (byte)(rgb >> 8);
        var b = (byte)rgb;
        return new Vector4(r / 255f, g / 255f, b / 255f, -1);
    }

    public static ParticleEffect FromProperties() {
        return new FountainEffect(null);
    }

}