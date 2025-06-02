using System.Collections.ObjectModel;
using System.Xml.Linq;
using Common;

namespace ContentBuilder;

public partial class Builder {

    public readonly string ContentPath;

    public readonly string OutputPath;

    public readonly ReadOnlyDictionary<string, string> OldHashes;

    public readonly Dictionary<string, string> CurrentHashes = [];

    public Builder(string contentPath, string outputPath) {
        ContentPath = contentPath;
        OutputPath = outputPath;
        OldHashes = LoadHashes();
    }

    public void Load() {
        Console.WriteLine("Content Builder Starting");
        
        
        var path = Path.Combine(ContentPath, "Content.xml");

        if (!File.Exists(path)) {
            throw new Exception($"Missing file 'Content.xml' at {ContentPath}");
        }
        
        ParseContentFile(path);
        
        WriteHashes();
        Console.WriteLine("Content Builder Finished");
    }

    private ReadOnlyDictionary<string, string> LoadHashes() {
        var hashes = new Dictionary<string, string>();
        var path = Path.Combine(ContentPath, "bin");
        var file = Path.Combine(path, "content.hash");

        if (!File.Exists(file)) {
            return new ReadOnlyDictionary<string, string>(hashes);
        }

        var lines = File.ReadAllLines(file);
        foreach (var line in lines) {
            var kvp = line.Split(';');

            if (kvp.Length < 2)
                continue;

            hashes[kvp[0]] = kvp[1];
        }

        return new ReadOnlyDictionary<string, string>(hashes);
    }

    private void WriteHashes() {
        var path = Path.Combine(ContentPath, "bin");
        var file = Path.Combine(path, "content.hash");
        File.Delete(file);
        File.WriteAllLines(file, CurrentHashes.Select(s => $"{s.Key};{s.Value}"));
    }

    private void ParseContentFile(string path) {
        var data = File.ReadAllText(path);
        var fullXml = XElement.Parse(data);
        var xmls = fullXml.Elements("Build");
        var verbose = fullXml.GetAttribute<bool>("Verbose");

        foreach (var xml in xmls) {
            Enum.TryParse(xml.GetAttribute<string>("mode"), out Mode mode);
            Enum.TryParse(xml.GetAttribute<string>("type"), out Type type);
            var ext = type == Type.Folder ? xml.GetAttribute<string>("ext") : "";
            var value = xml.Value;

            switch (mode, type) {
                case (Mode.Copy, Type.Folder):
                    CopyFolder(Path.Combine(ContentPath, value), ext, verbose);
                    break;
                case (Mode.Copy, Type.File):
                    CopyFile(Path.Combine(ContentPath, value));
                    break;
                case (Mode.Font, Type.Folder):
                    BuildFonts(Path.Combine(ContentPath, value), ext, verbose);
                    break;
                default:
                    Console.WriteLine($"Unsupported setup of '{mode} {type}' for {value}");
                    break;
            }
        }
    }
}