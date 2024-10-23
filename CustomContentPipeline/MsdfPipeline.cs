using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Common;
using Common.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace CustomContentPipeline;

public class MsdfSettings {
    public string FontPath;
    public int MaxOutlineSize;
    public string Characters;
}

public class MsdfResult {
    public string AtlasPath;
    public string LayoutPath;
}

[ContentImporter(".msdf", DisplayName = "MSDF Importer", DefaultProcessor = nameof(MsdfProcessor))]
public class MsdfImporter : ContentImporter<MsdfSettings> {
    public override MsdfSettings Import(string filename, ContentImporterContext context) {
        var settings = new MsdfSettings();
        var xml = XElement.Parse(File.ReadAllText(filename));

        settings.FontPath = Path.Combine(Directory.GetCurrentDirectory(), xml.GetValue("FontPath", ""));
        settings.MaxOutlineSize = xml.GetValue("MaxOutlineSize", 16);

        var list = "";
        foreach (var elem in xml.Elements("CharRange")) {
            var start = elem.GetAttribute<uint>("start");
            var end = elem.GetAttribute<uint>("end");
            if (end < start) {
                throw new Exception(
                    $"MSDF Importer - End character {(char) end} was lower value than start character {(char) start}");
            }

            list += $"[0x{start:x4}, 0x{end:x4}],";
        }

        settings.Characters = list[..^1];

        return settings;
    }
}

[ContentTypeWriter]
public class MsdfWriter : ContentTypeWriter<MsdfResult> {
    public override string GetRuntimeReader(TargetPlatform targetPlatform) {
        var type = typeof(MsdfReader);
        return $"{type.FullName}, {type.Assembly.GetName().Name}";
    }

    protected override void Write(ContentWriter output, MsdfResult result) {
        var atlas = File.ReadAllBytes(result.AtlasPath);
        output.Write(atlas.Length);
        output.Write(atlas);
        var json = File.ReadAllText(result.LayoutPath);
        output.Write(json);
    }
}

[ContentProcessor(DisplayName = "MSDF Processor")]
public class MsdfProcessor : ContentProcessor<MsdfSettings, MsdfResult> {
    public override MsdfResult Process(MsdfSettings input, ContentProcessorContext context) {
        var genPath = Path.Combine(Directory.GetCurrentDirectory(), "Fonts/msdf-atlas-gen-w64.exe");
        var workPath = Path.Combine(context.IntermediateDirectory, "Fonts");
        var font = Path.GetFileNameWithoutExtension(input.FontPath);
        if (!Directory.Exists(workPath)) {
            Directory.CreateDirectory(workPath);
        }

        var charsetPath = Path.Combine(workPath, $"{font}-charset.txt");
        var jsonPath = Path.Combine(workPath, $"{font}-layout.json");
        var atlasPath = Path.Combine(workPath, $"{font}-atlas.png");
        
        File.WriteAllText(charsetPath, input.Characters);

        if (!File.Exists(genPath)) {
            throw new Exception($"MsdfProcessor - Unable to fine msdfgen at {genPath}");
        }

        var startInfo = new ProcessStartInfo(genPath) {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            Arguments =
                $"-type mtsdf -font \"{input.FontPath}\" -imageout \"{atlasPath}\" -pots -charset \"{charsetPath}\" -size {64} -pxrange {input.MaxOutlineSize} -json \"{jsonPath}\" -yorigin top"
        };
        var process = System.Diagnostics.Process.Start(startInfo);
        if (process == null) {
            throw new InvalidOperationException("Could not start msdf-atlas-gen.exe");
        }

        process.WaitForExit();
        return new MsdfResult { LayoutPath = jsonPath, AtlasPath = atlasPath };
    }
}