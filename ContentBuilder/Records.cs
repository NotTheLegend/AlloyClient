using System.Xml.Linq;
using Assimp;
using Common;
using Common.Atlas;
using StbImageSharp;

namespace ContentBuilder;

public record struct FolderSettings(string Folder, string Ext);

public record struct FileSettings(string File);

public record ModelData(List<int> Indices, List<Vector3D> Vertices, List<Vector3D> Normals, List<Vector2D> UV, bool HasUv) {
    
    public void Write(BinaryWriter writer) {
        var len = Indices.Count;
        writer.Write(len);
        for (var i = 0; i < len; i++) {
            writer.Write(Indices[i]);
        }

        len = Vertices.Count;
        for (var i = 0; i < len; i++) {
            writer.Write(Vertices[i].X);
            writer.Write(Vertices[i].Y);
            writer.Write(Vertices[i].Z);
        }

        len = Normals.Count;
        for (var i = 0; i < len; i++) {
            writer.Write(Normals[i].X);
            writer.Write(Normals[i].Y);
            writer.Write(Normals[i].Z);
        }

        len = UV.Count;
        for (var i = 0; i < len; i++) {
            writer.Write(UV[i].X);
            writer.Write(UV[i].Y);
        }
        writer.Write(HasUv);
    }
}

public record FontData(byte[] Atlas, List<string> Types, string Json) {

    public void Write(BinaryWriter writer) {
        writer.Write(Atlas.Length);
        writer.Write(Atlas);
        
        writer.Write(Types.Count);
        foreach (var type in Types) {
            writer.Write(type);
        }
        
        //TODO move the jdoc parsing stuff to content builder
        writer.Write(Json);
    }
}

public record Paths(string Content, string Output);

public record StaticSheet(string File, string Lookup, int Width, int Height) {
    public static StaticSheet Parse(XElement xml, string path) {
        var file = Path.Combine(path, xml.Value);
        var lookup = xml.GetAttribute("name", "ERROR");
        var w = xml.GetAttribute("w", -1);
        var h = xml.GetAttribute("h", -1);
        return new StaticSheet(file, lookup, w, h);
    }
}

public record AnimatedSheet(string File, string Lookup, int Width, int Height, Grouping Grouping) {
    public static AnimatedSheet Parse(XElement xml, string path) {
        var file = Path.Combine(path, xml.Value);
        var lookup = xml.GetAttribute("name", "ERROR");
        var w = xml.GetAttribute("w", -1);
        var h = xml.GetAttribute("h", -1);
        Enum.TryParse(xml.GetAttribute("group", "Single"), out Grouping group) ;
        return new AnimatedSheet(file, lookup, w, h, group);
    }
}

public record struct Color(byte R, byte G, byte B, byte A);

public record Atlas(ImageResult Png, Dictionary<string, AtlasData[]> AtlasMapStatic, Dictionary<string, AnimationAtlasData[]> AtlasMapAnimation, Dictionary<string, Color[]> DominantColors) {
    public void Write(BinaryWriter writer) {
        Png.Write(writer);

        writer.Write(AtlasMapStatic.Count);

        foreach (var (key, value) in AtlasMapStatic) {
            writer.Write(key);
            writer.Write(value.Length);
            foreach (var data in value) {
                data.Write(writer);
            }
        }

        writer.Write(AtlasMapAnimation.Count);

        foreach (var (key, value) in AtlasMapAnimation) {
            writer.Write(key);
            writer.Write(value.Length);

            foreach (var anim in value) {
                writer.Write(anim.FaceRight.Length);
                foreach (var data in anim.FaceRight) {
                    data.Write(writer);
                }

                writer.Write(anim.FaceDown.Length);
                foreach (var data in anim.FaceDown) {
                    data.Write(writer);
                }

                writer.Write(anim.FaceUp.Length);
                foreach (var data in anim.FaceUp) {
                    data.Write(writer);
                }
            }
        }

        writer.Write(DominantColors.Count);

        foreach (var (key, value) in DominantColors) {
            writer.Write(key);
            writer.Write(value.Length);

            foreach (var color in value) {
                writer.Write(color.R);
                writer.Write(color.G);
                writer.Write(color.B);
                writer.Write(color.A);
            }
        }
    }
}