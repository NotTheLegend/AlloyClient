using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Common;
using Common.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace CustomContentPipeline;

public class FontFamilySettings {
    public string Group;
    public List<(FontType, string)> FontPaths;
    public int MaxOutlineSize;
    public string Characters;
}

public class FontFamilyResult(string atlasPath, List<FontType> types, string layoutPath) {
    public string AtlasPath = atlasPath;
    public List<FontType> Types = types;
    public string LayoutPath = layoutPath;
}

[ContentImporter(".mtsdf", DisplayName = "FontFamily Importer", DefaultProcessor = nameof(FontFamilyProcessor))]
public class FontFamilyImporter : ContentImporter<FontFamilySettings> {
    public override FontFamilySettings Import(string filename, ContentImporterContext context) {
        var settings = new FontFamilySettings();
        var xml = XElement.Parse(File.ReadAllText(filename));
        
        settings.Group = xml.GetAttribute<string>("group");
        settings.FontPaths = xml.Elements("FontPath").Select( i => (Enum.Parse<FontType>(i.GetAttribute("type", "Normal")), Path.Combine(Directory.GetCurrentDirectory(), i.Value))).ToList();
        settings.MaxOutlineSize = xml.GetValue("MaxOutlineSize", 16);

        var list = "";
        foreach (var elem in xml.Elements("CharRange")) {
            var start = elem.GetAttribute<uint>("start");
            var end = elem.GetAttribute<uint>("end");
            if (end < start) {
                throw new Exception($"MSDF Importer - End character {(char) end} was lower value than start character {(char) start}");
            }

            list += $"[0x{start:x4}, 0x{end:x4}],";
        }

        settings.Characters = list[..^1];

        return settings;
    }
}

[ContentTypeWriter]
public class FontFamilyWriter : ContentTypeWriter<FontFamilyResult> {
    public override string GetRuntimeReader(TargetPlatform targetPlatform) {
        var type = typeof(FontFamilyReader);
        return $"{type.FullName}, {type.Assembly.GetName().Name}";
    }

    protected override void Write(ContentWriter output, FontFamilyResult result) {
        var atlas = File.ReadAllBytes(result.AtlasPath);
        output.Write(atlas.Length);
        output.Write(atlas);
        
        output.Write(result.Types.Count);
        foreach (var type in result.Types) {
            output.Write((int)type);
        }
        
        var json = File.ReadAllText(result.LayoutPath);
        output.Write(json);
    }
}

[ContentProcessor(DisplayName = "FontFamily Processor")]
public class FontFamilyProcessor : ContentProcessor<FontFamilySettings, FontFamilyResult> {
    public override FontFamilyResult Process(FontFamilySettings input, ContentProcessorContext context) {
        var genPath = Path.Combine(Directory.GetCurrentDirectory(), "Fonts/msdf-atlas-gen-w64.exe");
        var workPath = Path.Combine(context.IntermediateDirectory, "Fonts");
        
        if (!Directory.Exists(workPath)) {
            Directory.CreateDirectory(workPath);
        }
        
        var charsetPath = Path.Combine(workPath, $"{input.Group}-charset.txt");
        var jsonPath = Path.Combine(workPath, $"{input.Group}-layout.json");
        var atlasPath = Path.Combine(workPath, $"{input.Group}-atlas.png");
        
        File.WriteAllText(charsetPath, input.Characters);

        if (!File.Exists(genPath)) {
            throw new Exception($"FontFamilyProcessor - Unable to fine msdfgen at {genPath}");
        }

        var types = new List<FontType>();
        var args = "";
        
        foreach (var kvp in input.FontPaths) {
            types.Add(kvp.Item1);
            if (args.Length > 0)
                args += " -and ";
            args += $"-font \"{ kvp.Item2}\"";
        }

        args += $" -type mtsdf -charset \"{charsetPath}\" -imageout \"{atlasPath}\" -dimensions 4096 4096 -size 64 -pxrange {input.MaxOutlineSize} -json \"{jsonPath}\" -yorigin top";
        
        Console.WriteLine(args);

        var startInfo = new ProcessStartInfo(genPath) {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            Arguments = args
        };
        var process = System.Diagnostics.Process.Start(startInfo);
        if (process == null) {
            throw new InvalidOperationException("Could not start msdf-atlas-gen.exe");
        }

        process.WaitForExit();
        return new FontFamilyResult(atlasPath, types, jsonPath);
    }
}