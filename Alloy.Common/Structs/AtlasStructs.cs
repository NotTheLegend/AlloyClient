using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace Alloy.Common.Structs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AtlasData : IEquatable<AtlasData> {

    public const float Padding = AtlasConfig.Padding;
    
    public float U;
    public float V;
    public float W;
    public float H;

    private AtlasData(float u, float v, float w, float h) {
        U = u;
        V = v;
        W = w;
        H = h;
    }

    public static AtlasData FromRaw(int u, int v, int w, int h) {
        return new AtlasData {
            U = u / AtlasConfig.AtlasWidth,
            V = v / AtlasConfig.AtlasHeight,
            W = w / AtlasConfig.AtlasWidth,
            H = h / AtlasConfig.AtlasHeight
        };
    }

    public void RemovePadding(uint pixels = 1) {
        U += Padding * pixels / AtlasConfig.AtlasWidth;
        V += Padding * pixels / AtlasConfig.AtlasHeight;
        W -= Padding * pixels * 2 / AtlasConfig.AtlasWidth;
        H -= Padding * pixels * 2 / AtlasConfig.AtlasHeight;
    }

    public Vector4 ToVector4(bool removePad = false) {
        if (removePad) RemovePadding();
        return new Vector4(U, V, W, H);
    }

    public AtlasData Rotate(int steps) {
        steps = (steps % 4 + 4) % 4;
        return steps switch {
            0 => new AtlasData(U, V, W, H),
            1 => new AtlasData(V + H, U, -H, W),
            2 => new AtlasData(U + W, V + H, -W, -H),
            3 => new AtlasData(V, U + W, H, -W),
            _ => new AtlasData(U, V, W, H)
        };
    }
    
    public int RawU() {
        return (int)(U * AtlasConfig.AtlasWidth);
    }

    public int RawV() {
        return (int)(V * AtlasConfig.AtlasHeight);
    }

    public int RawW() {
        return (int)(W * AtlasConfig.AtlasWidth);
    }

    public int RawH() {
        return (int)(H * AtlasConfig.AtlasHeight);
    }

    public static bool operator ==(AtlasData left, AtlasData right) {
        return left.Equals(right);
    }

    public static bool operator !=(AtlasData left, AtlasData right) {
        return !(left == right);
    }

    public bool Equals(AtlasData other) {
        return U.Equals(other.U) && V.Equals(other.V) && W.Equals(other.W) && H.Equals(other.H);
    }

    public override bool Equals(object obj) {
        return obj is AtlasData other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(U, V, W, H);
    }

    public override string ToString() {
        return $"U: {U}, V: {V}, W: {W}, H: {H}";
    }
}

public struct AnimationAtlasData {
    public AtlasData[] FaceRight;
    public AtlasData[] FaceDown;
    public AtlasData[] FaceUp;
}