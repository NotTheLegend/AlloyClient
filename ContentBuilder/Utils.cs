using Common.Atlas;
using StbImageSharp;

namespace ContentBuilder;

public static class Utils {
    public static void Write(this ImageResult image, BinaryWriter output) {
        output.Write(image.Width);
        output.Write(image.Height);
        output.Write(image.Data.Length);
        output.Write(image.Data);
    }

    public static void Write(this AtlasData data, BinaryWriter output) {
        output.Write(data.U);
        output.Write(data.V);
        output.Write(data.W);
        output.Write(data.H);
    }
}