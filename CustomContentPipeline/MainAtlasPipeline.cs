using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Common.Atlas;
using Common.Pipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using StbImageSharp;
using StbImageWriteSharp;
using StbRectPackSharp;
using ColorComponents = StbImageSharp.ColorComponents;

namespace CustomContentPipeline;

public class MainAtlasResult {
    public ImageResult MainAtlas;
    public readonly Dictionary<string, AtlasData[]> AtlasMapStatic = new();
    public readonly Dictionary<string, AnimationAtlasData[]> AtlasMapAnimation = new();
    public readonly Dictionary<string, Color[]> DominantColors = new();
}

[ContentImporter(".mainAtlasFlag", DisplayName = "Main Atlas Importer", DefaultProcessor = nameof(MainAtlasProcessor))]
public class MainAtlasImporter : ContentImporter<string> {
    public override string Import(string filename, ContentImporterContext context) => filename;
}

[ContentTypeWriter]
public class MainAtlasWriter : ContentTypeWriter<MainAtlasResult> {
    public override string GetRuntimeReader(TargetPlatform targetPlatform) {
        var type = typeof(MainAtlasReader);
        return $"{type.FullName}, {type.Assembly.GetName().Name}";
    }

    protected override void Write(ContentWriter output, MainAtlasResult result) {
        result.MainAtlas.Write(output);

        output.Write(result.AtlasMapStatic.Count);

        foreach (var (key, value) in result.AtlasMapStatic) {
            output.Write(key);
            output.Write(value.Length);
            foreach (var data in value) {
                data.Write(output);
            }
        }

        output.Write(result.AtlasMapAnimation.Count);

        foreach (var (key, value) in result.AtlasMapAnimation) {
            output.Write(key);
            output.Write(value.Length);

            foreach (var anim in value) {
                output.Write(anim.FaceRight.Length);
                foreach (var data in anim.FaceRight) {
                    data.Write(output);
                }

                output.Write(anim.FaceDown.Length);
                foreach (var data in anim.FaceDown) {
                    data.Write(output);
                }

                output.Write(anim.FaceUp.Length);
                foreach (var data in anim.FaceUp) {
                    data.Write(output);
                }
            }
        }

        output.Write(result.DominantColors.Count);

        foreach (var (key, value) in result.DominantColors) {
            output.Write(key);
            output.Write(value.Length);

            foreach (var color in value) {
                output.Write(color.R);
                output.Write(color.G);
                output.Write(color.B);
                output.Write(color.A);
            }
        }
    }
}

[ContentProcessor(DisplayName = "Main Atlas Processor")]
unsafe class MainAtlasProcessor : ContentProcessor<string, MainAtlasResult> {
    private const int AtlasWidth = (int) AtlasConfig.AtlasWidth;
    private const int AtlasHeight = (int) AtlasConfig.AtlasHeight;
    private const int Padding = (int) AtlasConfig.Padding;

    private readonly MainAtlasResult _result = new();
    private string _directory;
    private StbRectPack.stbrp_context _stbContext;

    private static readonly ImageResult Atlas = new() {
        Width = AtlasWidth,
        Height = AtlasHeight,
        SourceComp = ColorComponents.RedGreenBlueAlpha,
        Comp = ColorComponents.RedGreenBlueAlpha,
        Data = new byte[AtlasWidth * AtlasHeight * 4]
    };

    public override MainAtlasResult Process(string input, ContentProcessorContext context) {
        _directory = input.Replace("Atlas.mainAtlasFlag", "/Sheets/");
        var numNodes = AtlasWidth;
        _stbContext = new StbRectPack.stbrp_context(numNodes);

        fixed (StbRectPack.stbrp_context* contextPtr = &_stbContext) {
            StbRectPack.stbrp_init_target(contextPtr, AtlasWidth, AtlasHeight, _stbContext.all_nodes, numNodes);
        }

        AddImage("cursors", "cursors.png", 32, 32);
        AddImage("tileAlphaBlend", "AlphaBlendTile.png", 8, 8);
        AddImage("invisible", "invisible.png", 8, 8);
        AddImage("lofiChar8x8", "lofichar.png", 8, 8);
        AddImage("lofiChar16x16", "lofichar.png", 16, 16);
        AddImage("lofiChar16x8", "lofichar.png", 16, 8);
        AddImage("lofiChar216x8", "lofichar2.png", 16, 8);
        AddImage("lofiChar216x16", "lofichar2.png", 16, 16);
        AddImage("lofiChar28x8", "lofichar2.png", 8, 8);
        AddImage("lofiCharBig", "loficharbig.png", 16, 16);
        AddImage("lofiEnvironment", "lofienvironment.png", 8, 8);
        AddImage("lofiEnvironment2", "lofienvironment2.png", 8, 8);
        AddImage("lofiEnvironment3", "lofienvironment3.png", 8, 8);
        AddImage("redLootBag", "redlootbag.png", 8, 8);
        AddImage("lofiInterfaceBig", "lofiinterfacebig.png", 16, 16);
        AddImage("lofiObj", "lofiobj.png", 8, 8);
        AddImage("lofiObj2", "lofiobj2.png", 8, 8);
        AddImage("lofiObj3", "lofiobj3.png", 8, 8);
        AddImage("lofiObj4", "lofiobj4.png", 8, 8);
        AddImage("lofiObj5", "lofiobj5.png", 8, 8);
        AddImage("lofiObj6", "lofiobj6.png", 8, 8);
        AddImage("lofiObjBig", "lofiobjbig.png", 16, 16);
        AddImage("lofiObj40x40", "lofiobj40x40.png", 40, 40);
        AddImage("lofiProjs", "lofiprojs.png", 8, 8);
        AddImage("lofiProjsBig", "lofiprojsbig.png", 16, 16);
        AddImage("lofiParts", "lofiparts.png", 8, 8);
        AddImage("customObjects8x8", "customobjects8x8.png", 8, 8);
        AddImage("customObjects16x16", "customobjects16x16.png", 16, 16);
        AddImage("customObjects24x24", "customobjects24x24.png", 24, 24);
        AddImage("customObjects32x32", "customobjects32x32.png", 32, 32);
        AddImage("customObjects64x64", "customobjects64x64.png", 64, 64);
        AddImage("customObjects128x128", "customobjects128x128.png", 128, 128);
        AddImage("customObjects92x78", "customobjects92x78.png", 92, 78);
        AddImage("customObjects92x32", "customobjects92x32.png", 92, 32);
        AddImage("lofiObj7", "lofiobj7.png", 8, 8);
        AddImage("lostHallsObjects8x8", "losthallsobjects8x8.png", 8, 8);
        AddImage("lostHallsObjects16x16", "losthallsobjects16x16.png", 16, 16);
        AddImage("d3LofiObj", "d3lofiobj.png", 8, 8);
        AddImage("romanNumbers8x8", "romannumbers8x8.png", 8, 8);
        AddImage("shmittySheet", "shmittysheet.png", 8, 8);
        AddImage("shmittySheet16", "shmittysheet16.png", 16, 16);
        AddImage("shmittySheet32", "shmittysheet32.png", 32, 32);
        AddImage("darkRoomStars", "darkroomstars.png", 8, 8);
        AddImage("darkRoomStars2", "darkroomstars.png", 8, 8);
        AddImage("gPlusSheet", "gplussheet.png", 8, 8);
        AddImage("gPlusSheet16", "gplussheet16x16.png", 16, 16);
        AddImage("gPlusSheet32", "gplussheet32x32.png", 32, 32);
        AddImage("gPlusSheet64", "gplussheet64x64.png", 64, 64);
        AddImage("d1lofiObjBig", "d1lofiobjbig.png", 16, 16);
        AddImage("dungeonModifierIcons", "dungeonmodifiericons.png", 8, 8);

        AddAnimatedImage("chars8x8rBeach", "chars8x8rbeach.png", 8, 8);
        AddAnimatedImage("chars8x8dBeach", "chars8x8dbeach.png", 8, 8);
        AddAnimatedImage("chars8x8rLow1", "chars8x8rlow1.png", 8, 8);
        AddAnimatedImage("chars8x8rLow2", "chars8x8rlow2.png", 8, 8);
        AddAnimatedImage("chars8x8rMid", "chars8x8rmid.png", 8, 8);
        AddAnimatedImage("chars8x8rMid2", "chars8x8rmid2.png", 8, 8);
        AddAnimatedImage("chars8x8rHigh", "chars8x8rhigh.png", 8, 8);
        AddAnimatedImage("chars8x8rHero1", "chars8x8rhero1.png", 8, 8);
        AddAnimatedImage("chars8x8rHero2", "chars8x8rhero2.png", 8, 8);
        AddAnimatedImage("chars8x8dHero1", "chars8x8dhero1.png", 8, 8);
        AddAnimatedImage("chars16x16dMountains1", "chars16x16dmountains1.png", 16, 16);
        AddAnimatedImage("chars16x16dMountains2", "chars16x16dmountains2.png", 16, 16);
        AddAnimatedImage("chars8x8dEncounters", "chars8x8dencounters.png", 8, 8);
        AddAnimatedImage("chars8x8rEncounters", "chars8x8rencounters.png", 8, 8);
        AddAnimatedImage("chars16x8dEncounters", "chars16x8dencounters.png", 16, 8);
        AddAnimatedImage("chars16x16dEncounters", "chars16x16dencounters.png", 16, 16);
        AddAnimatedImage("chars16x16dEncounters2", "chars16x16dencounters2.png", 16, 16);
        AddAnimatedImage("chars16x16rEncounters", "chars16x16rencounters.png", 16, 16);

        AddAnimatedImage("players", "players.png", 8, 8, Grouping.Full);
        AddAnimatedImage("playerskins", "PlayerSkins.png", 8, 8, Grouping.Full);
        AddAnimatedImage("playerskins16", "PlayerSkins16.png", 16, 16, Grouping.Full);

        AddAnimatedImage("chars8x8rPets1", "chars8x8rpets1.png", 8, 8);
        AddAnimatedImage("customChars8x8", "customchars8x8.png", 8, 8);
        AddAnimatedImage("lostHallsChars8x8", "losthallschars8x8.png", 8, 8);
        AddAnimatedImage("lostHallsChars16x16", "losthallschars16x16.png", 16, 16);
        AddAnimatedImage("customChars16x16", "customchars16x16.png", 16, 16);
        AddAnimatedImage("customChars24x24", "customchars24x24.png", 24, 24);
        AddAnimatedImage("customChars32x32", "customchars32x32.png", 32, 32);
        AddAnimatedImage("d1Chars16x16r", "d1chars16x16r.png", 16, 16);
        AddAnimatedImage("d3Chars16x16r", "d3chars16x16r.png", 16, 16);
        AddAnimatedImage("d1Chars32x32r", "d1chars32x32r.png", 32, 32);
        AddAnimatedImage("d3Chars8x8r", "d3chars8x8r.png", 8, 8);
        AddAnimatedImage("d1Chars64x64r", "d1chars64x64r.png", 64, 64);
        AddAnimatedImage("customChars128x128", "customchars128x128.png", 128, 128);
        AddAnimatedImage("customChars102x64", "customchars102x64.png", 102, 64);
        AddAnimatedImage("customChars86x50", "customchars86x50.png", 86, 50);
        AddAnimatedImage("petsDivine", "petsdivine.png", 16, 16);
        
        // todo fix g sheets lmao
        //AddAnimatedImage("gPlusEntities8", "gPlusEntities8.png", 8, 8); // sheet height wrong
        //AddAnimatedImage("gPlusEntities16", "gPlusEntities16.png", 16, 16);// sheet height wrong
        AddAnimatedImage("gPlusEntities32", "gPlusEntities32.png", 32, 32);
        AddAnimatedImage("gPlusEntities64", "gPlusEntities64.png", 64, 64);
        //AddAnimatedImage("gPlusPlayer8", "gPlusPlayer8.png", 8, 8, Grouping.Full);// sheet height wrong
        //AddAnimatedImage("gPlusPlayer16", "gPlusPlayer16.png", 16, 16, Grouping.Full);// sheet height wrong
        //AddAnimatedImage("gPlusPlayer32", "gPlusPlayer32.png", 32, 32, Grouping.Full);//index out of bounds
        //AddAnimatedImage("gPlusPlayer64", "gPlusPlayer64.png", 64, 64, Grouping.Full);//index out of bounds


        // var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        // WriteAtlasToFile(Atlas, desktopPath + "/NewAtlas.png");

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

    private void AddImage(string sheetName, string imageName, int cutWidth, int cutHeight) {
        try {
            var image = ImageResult.FromMemory(File.ReadAllBytes(_directory + imageName));
            if (image.Width % cutWidth != 0) {
                throw new Exception($"Skipping image <{imageName}>, cutWidth <{cutWidth}> not a multiple of {image.Width}");
            }

            if (image.Height % cutHeight != 0) {
                throw new Exception($"Skipping image <{imageName}>, cutHeight <{cutHeight}> not a multiple of {image.Height}");
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

            _result.AtlasMapStatic[sheetName] = adList;

            var dominantColors = new Color[len];
            for (var i = 0; i < len; i++) {
                var rect = rectList[i];

                if (rect.w == 0 || rect.h == 0) {
                    continue;
                }

                var curAtlasX = rect.x + Padding;
                var curAtlasY = rect.y + Padding;
                var curSrcX = i * cutWidth % image.Width;
                var curSrcY = i * cutHeight / image.Width * cutHeight;
                var colorCounts = new Dictionary<Color, int>();

                for (var j = 0; j < cutSize; j++) {
                    var rowCount = j / cutWidth;
                    var rowIdx = j % cutWidth;
                    var atlasIdx = ((curAtlasY + rowCount) * AtlasWidth + curAtlasX + rowIdx) * 4;
                    var srcIdx = ((curSrcY + rowCount) * image.Width + curSrcX + rowIdx) * 4;
                    Array.Copy(image.Data, srcIdx, Atlas.Data, atlasIdx, 4);

                    if (image.Data[srcIdx + 3] <= 0) {
                        continue;
                    }

                    var rgba = new Color(image.Data[srcIdx], image.Data[srcIdx + 1], image.Data[srcIdx + 2], (byte) 255);

                    if (colorCounts.TryGetValue(rgba, out var value)) {
                        colorCounts[rgba] = ++value;
                    }
                    else {
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

            _result.DominantColors[sheetName] = dominantColors;
        }
        catch (Exception e) {
            Console.WriteLine($"Failed to add image <{imageName}>: {e.Message}");
        }
    }

    private void AddAnimatedImage(string sheetName, string imageName, int cutWidth, int cutHeight,
        Grouping grouping = Grouping.Single) {
        try {
            var image = ImageResult.FromMemory(File.ReadAllBytes(_directory + imageName));
            if (image.Width % cutWidth != 0) {
                throw new Exception($"Skipping image <{imageName}>, cutWidth <{cutWidth}> not a multiple of {image.Width}");
            }

            if (image.Height % cutHeight != 0) {
                throw new Exception($"Skipping image <{imageName}>, cutHeight <{cutHeight}> not a multiple of {image.Height}");
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

            fixed (StbRectPack.stbrp_rect* rectListPtr = rectList)
            fixed (StbRectPack.stbrp_context* contextPtr = &_stbContext) {
                var result = StbRectPack.stbrp_pack_rects(contextPtr, rectListPtr, len);
                if (result == 0) {
                    throw new Exception($"Failed to pack image <{imageName}>.");
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
                    Array.Copy(image.Data, imgIdx, Atlas.Data, atlasIdx, 4);
                }
            }

            var group = (int) grouping;
            var data = new AnimationAtlasData[rowsPerSheet / group];
            for (var i = 0; i < rectList.Length;) {
                var animData = new AnimationAtlasData();
                var idx = i / framesPerRow / group;
                for (var g = 0; g < group; g++) {
                    var frames = new AtlasData[framesPerRow];

                    for (var j = 0; j < framesPerRow; j++) {
                        var rect = rectList[i];
                        frames[j] = AtlasData.FromRaw(rect.x, rect.y, rect.w, rect.h);
                        i++;
                    }

                    if (grouping == Grouping.Single) {
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

            _result.AtlasMapAnimation[sheetName] = data;
        }
        catch (Exception e) {
            Console.WriteLine($"Failed to add image <{imageName}>: {e.Message}");
        }
    }

    private enum Grouping {
        Single = 1,
        Full = 3
    }
}