using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Alloy.Common;

namespace Alloy.ContentBuilder.Builders;

public static class FontBuilder {

    private static string _workPath;

    private static string _genPath;

    private static bool _notWindows;

    public static void Process(FolderSettings settings, Paths paths) {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            Console.WriteLine("User is not using Windows. Running required executables through Wine.");
            _notWindows = true;
        }
        
        Init();
        
        var files = Directory.GetFiles(settings.Folder, settings.Ext, SearchOption.AllDirectories);

        foreach (var file in files) {
            ProcessFile(file, paths);
        }
    }
    
    public static void Process(FileSettings settings, Paths paths) {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            Console.WriteLine("User is not using Windows. Running required executables through Wine.");
            _notWindows = true;
        }
        
        Init();
        
        ProcessFile(settings.File, paths);
    }

    private static void Init() {
        _workPath = Path.CombineAlt(AppDomain.CurrentDomain.BaseDirectory, "temp");
        _genPath = Path.CombineAlt(AppDomain.CurrentDomain.BaseDirectory, "msdf-atlas-gen-w64.exe");

        if (!File.Exists(_genPath)) {
            throw new Exception($"Missing atlas gen at {_genPath}");
        }
        
        if (!Directory.Exists(_workPath)) {
            Directory.CreateDirectory(_workPath);
        }
    }
    
    private static bool CheckHash(string file, List<(string, string)> fonts, Paths paths) {
        var allSame = HashManager.CheckFileHash(file, paths);
        
        foreach (var kvp in fonts) {
            allSame = HashManager.CheckFileHash(kvp.Item2, paths) && allSame;
        }
        
        return allSame;
    }

    private static void ProcessFile(string file, Paths paths) {
        var xml = XElement.Parse(File.ReadAllText(file));
        var group = xml.GetAttribute<string>("group");
        var fontPaths = xml.Elements("FontPath").Select( i => (i.GetAttribute("type", "Normal"), Path.CombineAlt(paths.Content, i.Value))).ToList();
        var outlineSize = xml.GetValue("MaxOutlineSize", 16);
        
        if (CheckHash(file, fontPaths, paths)) {
            Console.WriteLine($"Skipping font group: {Path.GetFileName(file)}");
            return;
        }
        
        var charSet = "";
        foreach (var elem in xml.Elements("CharRange")) {
            var start = elem.GetAttribute<uint>("start");
            var end = elem.GetAttribute<uint>("end");
            if (end < start) {
                throw new Exception($"MSDF Importer - End character {(char) end} was lower value than start character {(char) start}");
            }

            charSet += $"[0x{start:x4}, 0x{end:x4}],";
        }
            
        var data = BuildFontAtlas(group, fontPaths, outlineSize, charSet);
        
        Write(data, file, paths);
        
        Console.WriteLine($"Updating font group: {Path.GetFileName(file)}");
    }
    
    private static FontData BuildFontAtlas(string group, List<(string, string)> fonts, int outlineSize, string charSet) {
        var charsetPath = Path.CombineAlt(_workPath, $"{group}-charset.txt");
        var jsonPath = Path.CombineAlt(_workPath, $"{group}-layout.json");
        var atlasPath = Path.CombineAlt(_workPath, $"{group}-atlas.png");
        
        File.WriteAllText(charsetPath, charSet);
        
        var types = new List<string>();
        var args = "";
        
        foreach (var kvp in fonts) {
            types.Add(kvp.Item1);
            if (args.Length > 0)
                args += " -and ";
            args += $"-font \"{ kvp.Item2}\"";
        }

        args += $" -type mtsdf -charset \"{charsetPath}\" -imageout \"{atlasPath}\" -dimensions 4096 4096 -size 64 -pxrange {outlineSize} -json \"{jsonPath}\" -yorigin top";
        
        
        ProcessStartInfo startInfo;
        if (_notWindows) {
            startInfo = new ProcessStartInfo("wine") {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = _genPath + " " + args
            };
        } else {
            startInfo = new ProcessStartInfo(_genPath) {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = args
            };
        }

        var process = System.Diagnostics.Process.Start(startInfo);
        if (process == null) {
            throw new InvalidOperationException("Could not start msdf-atlas-gen.exe");
        }

        process.WaitForExit();

        return new FontData(File.ReadAllBytes(atlasPath), types, File.ReadAllText(jsonPath));
    }

    private static void Write(FontData data, string file, Paths paths) {
        var newFile = Path.ChangeExtension(file.Replace(paths.Content, paths.Output), ".msdf");
        Directory.CreateDirectory(Path.GetDirectoryName(newFile)!);
        
        using var stream = File.Create(newFile);
        using var writer = new BinaryWriter(stream);
        data.Write(writer);
    }
}