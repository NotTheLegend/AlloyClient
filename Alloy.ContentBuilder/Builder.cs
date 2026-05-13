using System.Diagnostics;
using System.Xml.Linq;
using Alloy.Common;
using Alloy.ContentBuilder.Builders;

namespace Alloy.ContentBuilder;

public static class Builder {

    public static bool Verbose;

    public static void Run(Paths paths) {
        var timer = Stopwatch.StartNew();
        Console.WriteLine("===== Content Builder Starting =====\n");
        
        
        var content = Path.CombineAlt(paths.Content, "Content.xml");

        if (!File.Exists(content)) {
            throw new Exception($"Missing file 'Content.xml' at {paths.Content}");
        }
        
        ParseBuilder(content, paths);
        
        Console.WriteLine($"\n===== Content Builder Finished in {timer.Elapsed} =====");
    }

    private static void ParseBuilder(string content, Paths paths) {
        var data = File.ReadAllText(content);
        var fullXml = XElement.Parse(data);
        Verbose = fullXml.GetAttribute<bool>("Verbose");
        var builders = fullXml.Elements("Builder");

        foreach (var build in builders) {
            var t = build.GetAttribute<string>("type");
            Enum.TryParse(t, out Type type);

            if (type == Type.Error) {
                Console.WriteLine($"Unsupported builder '{type}' from '{t}'");
                continue;
            }

            foreach (var loc in build.Elements()) {
                switch (loc.Name.LocalName) {
                    case "File":
                        RunBuilder(type, paths, new FileSettings(Path.CombineAlt(paths.Content, loc.Value)));
                        break;
                    case "Folder":
                        var ext = loc.GetAttribute("ext", "*.*");
                        RunBuilder(type, paths, new FolderSettings(Path.CombineAlt(paths.Content, loc.Value), ext));
                        break;
                    default:
                        Console.WriteLine($"Unknown location type of '{loc.Name.LocalName}'. Must be 'File' or 'Folder' ");
                        break;
                }
            }
        }
    }

    private static void RunBuilder(Type type, Paths paths, FileSettings settings) {
        switch (type) {
            case Type.Copy:
                CopyBuilder.Process(settings, paths);
                break;
            case Type.Font:
                FontBuilder.Process(settings, paths);
                break;
            case Type.Fbx:
                FbxBuilder.Process(settings, paths);
                break;
            case Type.Atlas:
                AtlasBuilder.Process(settings, paths);
                break;
            default:
                Console.WriteLine($"Unsupported builder setting: 'File' for {type}");
                break;
        }
    }

    private static void RunBuilder(Type type, Paths paths, FolderSettings settings) {
        switch (type) {
            case Type.Copy:
                CopyBuilder.Process(settings, paths);
                break;
            case Type.Font:
                FontBuilder.Process(settings, paths);
                break;
            case Type.Fbx:
                FbxBuilder.Process(settings, paths);
                break;
            default:
                Console.WriteLine($"Unsupported builder setting: 'Folder' for {type}");
                break;
        }
    }

    public static void Copy(Paths paths) {
        var files = Directory.GetFiles(paths.Content, "*.*", SearchOption.AllDirectories);

        foreach (var file in files) {
            var newPath = file.Replace(paths.Content, paths.Output);
            Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
            File.Copy(file, newPath, true);
        }
    }
}