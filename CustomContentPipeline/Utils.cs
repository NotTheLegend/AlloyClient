using Common.Atlas;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using StbImageSharp;

namespace CustomContentPipeline;

public static class Utils {
    public static void Write(this ImageResult image, ContentWriter output) {
        output.Write(image.Width);
        output.Write(image.Height);
        output.Write(image.Data.Length);
        output.Write(image.Data);
    }

    public static void Write(this AtlasData data, ContentWriter output) {
        output.Write(data.U);
        output.Write(data.V);
        output.Write(data.W);
        output.Write(data.H);
    }
}