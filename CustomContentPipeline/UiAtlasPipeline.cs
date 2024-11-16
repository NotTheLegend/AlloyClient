using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Common.Atlas;
using Common.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using StbImageSharp;
using StbImageWriteSharp;
using StbRectPackSharp;
using ColorComponents = StbImageSharp.ColorComponents;

namespace CustomContentPipeline;

public class UiAtlasResult {
    public ImageResult MainAtlas;
    public readonly Dictionary<string, AtlasData[]> AtlasMapFull = new();
}

[ContentImporter(".uiAtlasFlag", DisplayName = "Ui Atlas Importer", DefaultProcessor = nameof(UiAtlasProcessor))]
public class UiAtlasImporter : ContentImporter<string> {
    public override string Import(string filename, ContentImporterContext context) => filename;
}

[ContentTypeWriter]
public class UiAtlasWriter : ContentTypeWriter<UiAtlasResult> {
    public override string GetRuntimeReader(TargetPlatform targetPlatform) {
        var type = typeof(UiAtlasReader);
        return $"{type.FullName}, {type.Assembly.GetName().Name}";
    }

    protected override void Write(ContentWriter output, UiAtlasResult result) {
        result.MainAtlas.Write(output);

        output.Write(result.AtlasMapFull.Count);

        foreach (var (key, value) in result.AtlasMapFull) {
            output.Write(key);
            output.Write(value.Length);
            foreach (var data in value)
                data.Write(output);
        }
    }
}

[ContentProcessor(DisplayName = "Ui Atlas Processor")]
unsafe class UiAtlasProcessor : ContentProcessor<string, UiAtlasResult> {
    private const int AtlasWidth = (int) AtlasConfig.AtlasWidth;
    private const int AtlasHeight = (int) AtlasConfig.AtlasHeight;
    private const int Padding = (int) AtlasConfig.Padding;

    private UiAtlasResult _result = new();
    private string _directory;
    private StbRectPack.stbrp_context _stbContext;

    private static readonly ImageResult Atlas = new() {
        Width = AtlasWidth,
        Height = AtlasHeight,
        SourceComp = ColorComponents.RedGreenBlueAlpha,
        Comp = ColorComponents.RedGreenBlueAlpha,
        Data = new byte[AtlasWidth * AtlasHeight * 4]
    };

    public override UiAtlasResult Process(string input, ContentProcessorContext context) {
        _directory = input.Replace("AtlasUi.uiAtlasFlag", "/Ui/");
        var numNodes = AtlasWidth;
        _stbContext = new StbRectPack.stbrp_context(numNodes);

        fixed (StbRectPack.stbrp_context* contextPtr = &_stbContext) {
            StbRectPack.stbrp_init_target(contextPtr, AtlasWidth, AtlasHeight, _stbContext.all_nodes, numNodes);
        }
        
        AddImage("bar1", "bar1.png");
        AddImage("bar2", "bar2.png");
        AddImage("textBox", "TextBox.png");
        AddImage("tooltipBackground", "TooltipBackground.png");
        
        AddImage("BlackCircle", "BlackCircle.png");
        AddImage("CharacterList/StarGraphic", "CharacterList/StarGraphic.png");
        
        AddImage("ScrollBar/ScrollBarBackground", "ScrollBar/ScrollBarBackground.png");
        AddImage("ScrollBar/ScrollBarHandle", "ScrollBar/ScrollBarHandle.png");
        
        // var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        // WriteAtlasToFile(Atlas, desktopPath + "/UiAtlas.png");

        _stbContext.Dispose();
        _result.MainAtlas = Atlas;
        return _result;
    }

    private static void WriteAtlasToFile(ImageResult atlas, string path) {
        var width = atlas.Width;
        var height = atlas.Height;
        var comp = (StbImageWriteSharp.ColorComponents) atlas.Comp;
        var writer = new ImageWriter();
        using var fileStream = new FileStream(path, FileMode.Create);
        writer.WritePng(atlas.Data, width, height, comp, fileStream);
    }

    private static bool CutHaveData(ImageResult image, int startX, int startY, int width, int height) {
        for (var y = startY; y < startY + height; y++) {
            for (var x = startX; x < startX + width; x++) {
                if (image.Data[(y * image.Width + x) * 4 + 3] != 0) {
                    return true;
                }
            }
        }

        return false;
    }

    private void AddImage(string lookup, string imageName, int cutWidth = -1, int cutHeight = -1) {
        var image = ImageResult.FromMemory(File.ReadAllBytes(_directory + imageName));
        if (cutWidth == -1) {
            cutWidth = image.Width;
        }

        if (cutHeight == -1) {
            cutHeight = image.Height;
        }
        
        if (image.Width % cutWidth != 0) {
            throw new Exception($"Skipping image <{imageName}>, cutWidth <{cutWidth}> not a multiple of {image.Width}");
        }
        
        if (image.Height % cutHeight != 0) {
            throw new Exception($"Skipping image <{imageName}>, cutHeight <{cutHeight}> not a multiple of {image.Height}");
        }

        if (image.Width * image.Height * 4 != image.Data.Length) {
            throw new Exception($"Failed on image <{imageName}>, pngs must be in TruecolorAlpha format");
        }
        
        var cutSize = cutWidth * cutHeight;
        var len = image.Width * image.Height / cutSize;
        var rectList = new StbRectPack.stbrp_rect[len];
        var adList = new AtlasData[len];
        for (var i = 0; i < len; i++) {
            var curSrcX = i * cutWidth % image.Width;
            var curSrcY = i * cutHeight / image.Width * cutHeight;
            if (CutHaveData(image, curSrcX, curSrcY, cutWidth, cutHeight)) {
                rectList[i].w = cutWidth + Padding * 2;
                rectList[i].h = cutHeight + Padding * 2;
            }
            else {
                rectList[i].w = 0;
                rectList[i].h = 0;
            }
        }

        fixed (StbRectPack.stbrp_rect* rectListPtr = rectList)
        fixed (StbRectPack.stbrp_context* contextPtr = &_stbContext) {
            var result = StbRectPack.stbrp_pack_rects(contextPtr, rectListPtr, len);
            if (result == 0) {
                throw new Exception($"Failed to pack image <{imageName}>.");
            }
        }

        for (var i = 0; i < len; i++) {
            var rect = rectList[i];
            adList[i] = AtlasData.FromRaw(rect.x, rect.y, rect.w, rect.h);
        }

        _result.AtlasMapFull[lookup] = adList;

        for (var i = 0; i < len; i++) {
            var rect = rectList[i];

            if (rect.w == 0 || rect.h == 0) {
                continue;
            }

            var curAtlasX = rect.x + Padding;
            var curAtlasY = rect.y + Padding;
            var curSrcX = i * cutWidth % image.Width;
            var curSrcY = i * cutHeight / image.Width * cutHeight;

            for (var j = 0; j < cutSize; j++) {
                var rowCount = j / cutWidth;
                var rowIdx = j % cutWidth;
                var atlasIdx = ((curAtlasY + rowCount) * AtlasWidth + curAtlasX + rowIdx) * 4;
                var srcIdx = ((curSrcY + rowCount) * image.Width + curSrcX + rowIdx) * 4;
                Array.Copy(image.Data, srcIdx, Atlas.Data, atlasIdx, 4);
            }
        }
    }
}