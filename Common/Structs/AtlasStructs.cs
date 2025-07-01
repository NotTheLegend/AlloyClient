using Microsoft.Xna.Framework;

namespace Common.Atlas;

public struct AtlasData {
    public float U;
    public float V;
    public float W;
    public float H;

    public static AtlasData FromRaw(int u, int v, int w, int h) {
        return new AtlasData {
            U = u / AtlasConfig.AtlasWidth,
            V = v / AtlasConfig.AtlasHeight,
            W = w / AtlasConfig.AtlasWidth,
            H = h / AtlasConfig.AtlasHeight
        };
    }

    public void RemovePadding() {
        U += AtlasConfig.Padding / AtlasConfig.AtlasWidth;
        V += AtlasConfig.Padding / AtlasConfig.AtlasHeight;
        W -= AtlasConfig.Padding * 2 / AtlasConfig.AtlasWidth;
        H -= AtlasConfig.Padding * 2 / AtlasConfig.AtlasHeight;
    }

    public Vector4 ToVector4(bool removePad = false) {
        if (removePad) RemovePadding();
        return new Vector4(U, V, W, H);
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
        return left.U == right.U && left.V == right.V && left.W == right.W && left.H == right.H;
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