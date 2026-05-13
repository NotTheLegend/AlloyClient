using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Alloy.Common;
using Alloy.Common.Structs;
using StbImageSharp;
using StbImageWriteSharp;
using StbRectPackSharp;
using ColorComponents = StbImageSharp.ColorComponents;

namespace Alloy.ContentBuilder.Builders;

public static class AtlasBuilder {
    
    private const int AtlasWidth = (int) AtlasConfig.AtlasWidth;
    private const int AtlasHeight = (int) AtlasConfig.AtlasHeight;
    private const int Padding = (int) AtlasConfig.Padding;

    public static void Process(FileSettings settings, Paths paths) {
        var name = Path.GetFileName(settings.File);
        
        var mainXml = XElement.Parse(File.ReadAllText(settings.File!));
        //var mask = mainXml.GetAttribute("mask", false);
        var folder = Path.CombineAlt(paths.Content, mainXml.GetAttribute("source", ""));

        var staticSheets = new List<StaticSheet>();
        var animatedSheets = new List<AnimatedSheet>();

        foreach (var xml in mainXml.Elements()) {
            switch (xml.Name.LocalName) {
                case "Image":
                    staticSheets.Add(StaticSheet.Parse(xml, folder));
                    break;
                case "Animated":
                    animatedSheets.Add(AnimatedSheet.Parse(xml, folder));
                    break;
                default:
                    Console.WriteLine($"Unknown atlas type of '{xml.Name.LocalName}'. Must be 'Image' or 'Animated'");
                    break;
            }
        }

        var isSame = CheckHash(settings.File, staticSheets, animatedSheets, paths);

        if (isSame) {
            Console.WriteLine($"Skipping: {name}");
            return;
        }
        
        var atlas = BuildAtlas(staticSheets, animatedSheets, name!.Split('.')[0]);
        
        Write(atlas, settings.File, paths);

        Console.WriteLine($"Building: {name}");
    }
    
    private static bool CheckHash(string mainFile, List<StaticSheet> sheets1, List<AnimatedSheet> sheets2, Paths paths) {
        var allSame = HashManager.CheckFileHash(mainFile, paths);

        foreach (var sheet in sheets1) {
            allSame = HashManager.CheckFileHash(sheet.File, paths) && allSame;
        }
        
        foreach (var sheet in sheets2) {
            allSame = HashManager.CheckFileHash(sheet.File, paths) && allSame;
        }
        
        return allSame;
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
    private static unsafe Atlas BuildAtlas(List<StaticSheet> statics, List<AnimatedSheet> animated, string name) {
        var numNodes = AtlasWidth;
        var stbContext = new StbRectPack.stbrp_context(numNodes);

        var png = new ImageResult {
            Width = AtlasWidth,
            Height = AtlasHeight,
            SourceComp = ColorComponents.RedGreenBlueAlpha,
            Comp = ColorComponents.RedGreenBlueAlpha,
            Data = new byte[AtlasWidth * AtlasHeight * 4]
        };

        var atlas = new Atlas(png, new Dictionary<string, AtlasData[]>(), new Dictionary<string, AnimationAtlasData[]>(), new Dictionary<string, Color[]>());

        StbRectPack.stbrp_init_target(&stbContext, AtlasWidth, AtlasHeight, stbContext.all_nodes, numNodes);

        foreach (var sheet in statics) {
            try {
                ParseSheet(&stbContext, sheet, atlas);
            } catch (Exception e) {
                Console.WriteLine($"Failed to add image <{sheet.Lookup}>: {e.Message}");
            }
        }
        
        foreach (var sheet in animated) {
            try {
                ParseAnimated(&stbContext, sheet, atlas);
            } catch (Exception e) {
                Console.WriteLine($"Failed to add image <{sheet.Lookup}>: {e.Message}");
            }
        }
        
        // this whole thing is jank and i might deal with it later

        var path = Path.CombineAlt(AppDomain.CurrentDomain.BaseDirectory, $"temp/{name}.png");
        
        var width = png.Width;
        var height = png.Height;
        var comp = (StbImageWriteSharp.ColorComponents) png.Comp;
        var writer = new ImageWriter();
        using (var fileStream = new FileStream(path, FileMode.Create)) {
            writer.WritePng(png.Data, width, height, comp, fileStream);
        }
        
        stbContext.Dispose();

        atlas.File = File.ReadAllBytes(path);

        return atlas;
    }

    private static void Write(Atlas atlas, string file, Paths paths) {
        var newFile = Path.ChangeExtension(file.Replace(paths.Content, paths.Output), ".atlas");
        Directory.CreateDirectory(Path.GetDirectoryName(newFile)!);

        using var stream = File.Create(newFile);
        using var writer = new BinaryWriter(stream);
        atlas.Write(writer);
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

    private static unsafe void ParseSheet(StbRectPack.stbrp_context* contextPtr, StaticSheet sheet, Atlas atlas) {
        var image = ImageResult.FromMemory(File.ReadAllBytes(sheet.File));
        var cutWidth = sheet.Width;
        var cutHeight = sheet.Height;
        
        if (cutWidth == -1) {
            cutWidth = image.Width;
        }

        if (cutHeight == -1) {
            cutHeight = image.Height;
        }

        if (image.Width % cutWidth != 0) {
            throw new Exception($"Skipping image <{sheet.Lookup}>, cutWidth <{cutWidth}> not a multiple of {image.Width}");
        }

        if (image.Height % cutHeight != 0) {
            throw new Exception($"Skipping image <{sheet.Lookup}>, cutHeight <{cutHeight}> not a multiple of {image.Height}");
        }

        var cutSize = cutWidth * cutHeight;
        var len = image.Width * image.Height / cutSize;
        var rectList = new StbRectPack.stbrp_rect[len];
        var adList = new AtlasData[len];
        for (var i = 0; i < len; i++) {
            var curSrcX = i * cutWidth % image.Width;
            var curSrcY = i * cutWidth / image.Width * cutHeight;

            if (CutHaveData(image, curSrcX, curSrcY, cutWidth, cutHeight)) {
                rectList[i].w = cutWidth + Padding * 2;
                rectList[i].h = cutHeight + Padding * 2;
            } else {
                rectList[i].w = 0;
                rectList[i].h = 0;
            }
        }

        fixed (StbRectPack.stbrp_rect* rectListPtr = rectList) {
            var result = StbRectPack.stbrp_pack_rects(contextPtr, rectListPtr, len);
            if (result == 0) {
                throw new Exception($"Failed to pack image <{sheet.Lookup}>.");
            }
        }

        for (var i = 0; i < len; i++) {
            var rect = rectList[i];
            adList[i] = AtlasData.FromRaw(rect.x, rect.y, rect.w, rect.h);
        }

        atlas.AtlasMapStatic[sheet.Lookup] = adList;

        var dominantColors = new Color[len];
        for (var i = 0; i < len; i++) {
            var rect = rectList[i];

            if (rect.w == 0 || rect.h == 0) {
                continue;
            }

            var curAtlasX = rect.x + Padding;
            var curAtlasY = rect.y + Padding;
            var curSrcX = i * cutWidth % image.Width;
            var curSrcY = i * cutWidth / image.Width * cutHeight;
            var colorCounts = new Dictionary<Color, int>();

            for (var j = 0; j < cutSize; j++) {
                var rowCount = j / cutWidth;
                var rowIdx = j % cutWidth;
                var atlasIdx = ((curAtlasY + rowCount) * AtlasWidth + curAtlasX + rowIdx) * 4;
                var srcIdx = ((curSrcY + rowCount) * image.Width + curSrcX + rowIdx) * 4;
                Array.Copy(image.Data, srcIdx, atlas.Png.Data, atlasIdx, 4);

                if (image.Data[srcIdx + 3] <= 0) {
                    continue;
                }

                var rgba = new Color(image.Data[srcIdx], image.Data[srcIdx + 1], image.Data[srcIdx + 2], (byte) 255);

                if (colorCounts.TryGetValue(rgba, out var value)) {
                    colorCounts[rgba] = ++value;
                } else {
                    colorCounts[rgba] = 1;
                }
            }

            var max = 0;
            foreach (var (key, value) in colorCounts) {
                if (value <= max) {
                    continue;
                }

                dominantColors[i] = key;
                max = value;
            }
        }

        atlas.DominantColors[sheet.Lookup] = dominantColors;
    }

    private static unsafe void ParseAnimated(StbRectPack.stbrp_context* contextPtr, AnimatedSheet sheet, Atlas atlas) {
        var image = ImageResult.FromMemory(File.ReadAllBytes(sheet.File));
        var cutWidth = sheet.Width;
        var cutHeight = sheet.Height;
        
        if (cutWidth == -1) {
            cutWidth = image.Width;
        }

        if (cutHeight == -1) {
            cutHeight = image.Height;
        }
        
        if (image.Width % cutWidth != 0) {
            throw new Exception($"Skipping image <{sheet.Lookup}>, cutWidth <{cutWidth}> not a multiple of {image.Width}");
        }

        if (image.Height % cutHeight != 0) {
            throw new Exception($"Skipping image <{sheet.Lookup}>, cutHeight <{cutHeight}> not a multiple of {image.Height}");
        }

        var framesPerRow = image.Width / cutWidth - 1;
        var rowsPerSheet = image.Height / cutHeight;
        var len = framesPerRow * rowsPerSheet;
        var rectList = new StbRectPack.stbrp_rect[len];
        for (var r = 0; r < rowsPerSheet; r++) {
            for (var f = 0; f < framesPerRow; f++) {
                var srcX = f * cutWidth;
                var srcY = r * cutHeight;
                var idx = f + framesPerRow * r;
                var frameWidth = f == framesPerRow - 1 ? cutWidth * 2 : cutWidth;

                rectList[idx].w = 0;
                rectList[idx].h = 0;

                if (CutHaveData(image, srcX, srcY, frameWidth, cutHeight)) {
                    rectList[idx].w = frameWidth + Padding * 2;
                    rectList[idx].h = cutHeight + Padding * 2;
                }
            }
        }

        fixed (StbRectPack.stbrp_rect* rectListPtr = rectList) {
            var result = StbRectPack.stbrp_pack_rects(contextPtr, rectListPtr, len);
            if (result == 0) {
                throw new Exception($"Failed to pack image <{sheet.Lookup}>.");
            }
        }

        for (var i = 0; i < len; i++) {
            var rect = rectList[i];
            if (rect.w == 0 || rect.h == 0) {
                continue;
            }

            var atlasX = rect.x + Padding;
            var atlasY = rect.y + Padding;
            var recW = rect.w - Padding * 2;
            var recH = rect.h - Padding * 2;
            var count = recW * recH;

            var frameIdx = i % framesPerRow;
            var rowIdx = i / framesPerRow;
            var imgX = frameIdx * cutWidth;
            var imgY = rowIdx * cutHeight;

            for (var p = 0; p < count; p++) {
                var px = p % recW;
                var py = p / recW;
                var imgIdx = ((imgY + py) * image.Width + imgX + px) * 4;
                var atlasIdx = ((atlasY + py) * AtlasWidth + atlasX + px) * 4;
                Array.Copy(image.Data, imgIdx, atlas.Png.Data, atlasIdx, 4);
            }
        }

        var group = (int)sheet.Grouping;
        var data = new AnimationAtlasData[rowsPerSheet / group];
        for (var i = 0; i < rectList.Length;) {
            var animData = new AnimationAtlasData();
            var idx = i / framesPerRow / group;
            for (var g = 0; g < group; g++) {
                var frames = new AtlasData[framesPerRow];

                for (var j = 0; j < framesPerRow; j++) {
                    var rect = rectList[i];
                    frames[j] = AtlasData.FromRaw(rect.x, rect.y, rect.w, rect.h);
                    if (j == 2 && (rect.w == 0 || rect.h == 0)) {
                        frames[j] = frames[0];
                    }
                    i++;
                }

                if (sheet.Grouping == Grouping.Single) {
                    animData.FaceRight = frames;
                    animData.FaceDown = frames;
                    animData.FaceUp = frames;
                    continue;
                }

                switch (g) {
                    case 0:
                        animData.FaceRight = frames;
                        break;
                    case 1:
                        animData.FaceDown = frames;
                        break;
                    case 2:
                        animData.FaceUp = frames;
                        break;
                }
            }

            data[idx] = animData;
        }

        atlas.AtlasMapAnimation[sheet.Lookup] = data;
    }
}