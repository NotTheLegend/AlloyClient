using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml.Linq;
using Common;

namespace ContentBuilder;

public partial class Builder {

    private void BuildFonts(string path, string ext, bool verbose) {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            Console.WriteLine("SKIPPING FONTS, NON SUPPORTED OS USED");
            return;
        }
        
        var workPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
        var genPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "msdf-atlas-gen-w64.exe");

        if (!File.Exists(genPath)) {
            throw new Exception($"Missing atlas gen at {genPath}");
        }
        
        if (!Directory.Exists(workPath)) {
            Directory.CreateDirectory(workPath);
        }
        
        var files = Directory.GetFiles(path, ext, SearchOption.AllDirectories);

        foreach (var file in files) {
            var xml = XElement.Parse(File.ReadAllText(file));
            var group = xml.GetAttribute<string>("group");
            //todo replace contentpath
            var fontPaths = xml.Elements("FontPath").Select( i => (i.GetAttribute("type", "Normal"), Path.Combine(ContentPath, i.Value))).ToList();
            var outlineSize = xml.GetValue("MaxOutlineSize", 16);

            if (DoFontHash(file, fontPaths)) {
                Console.WriteLine($"Skipping font group: {Path.GetFileName(file)}");
                continue;
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
            
            BuildFontAtlas(workPath, genPath, group, fontPaths, outlineSize, charSet);
            Console.WriteLine($"Updating font group: {Path.GetFileName(file)}");
        }
    }

    private bool DoFontHash(string file, List<(string, string)> fonts) {
        var allSame = true;
        
        
        var data = File.ReadAllBytes(file);
        var md5 = MD5.HashData(data);
        var hash = Convert.ToBase64String(md5);
        var name = Path.GetFileName(file);
        
        CurrentHashes[name] = hash;
        
        if (OldHashes.TryGetValue(name, out var oldHash)) {
            allSame = oldHash == hash;
        } else {
            allSame = false;
        }
        
        foreach (var kvp in fonts) {
            data = File.ReadAllBytes(kvp.Item2);
            md5 = MD5.HashData(data);
            hash = Convert.ToBase64String(md5);
            name = Path.GetFileName(kvp.Item2);

            CurrentHashes[name] = hash;
            
            if (OldHashes.TryGetValue(name, out oldHash)) {
                allSame = allSame && oldHash == hash;
            } else {
                allSame = false;
            }
        }
        return allSame;
    }

    private void BuildFontAtlas(string workPath, string genPath, string group, List<(string, string)> fonts, int outlineSize, string charSet) {
        var charsetPath = Path.Combine(workPath, $"{group}-charset.txt");
        var jsonPath = Path.Combine(workPath, $"{group}-layout.json");
        var atlasPath = Path.Combine(workPath, $"{group}-atlas.png");
        
        File.WriteAllText(charsetPath, charSet);
        
        var args = "";
        
        foreach (var kvp in fonts) {
            if (args.Length > 0)
                args += " -and ";
            args += $"-font \"{ kvp.Item2}\"";
        }

        args += $" -type mtsdf -charset \"{charsetPath}\" -imageout \"{atlasPath}\" -dimensions 4096 4096 -size 64 -pxrange {outlineSize} -json \"{jsonPath}\" -yorigin top";
        
        var startInfo = new ProcessStartInfo(genPath) {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            Arguments = args
        };
        var process = Process.Start(startInfo);
        if (process == null) {
            throw new InvalidOperationException("Could not start msdf-atlas-gen.exe");
        }

        process.WaitForExit();
        
        //todo write to output
    }
    
}