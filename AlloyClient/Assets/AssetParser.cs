using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Alloy.Engine.Common;
using AlloyClient.Assets.Libraries;
using AlloyClient.Assets.XmlStructs;
using Alloy.Common;
using Alloy.Common.Structs;
using AlloyClient.Utils;
using Microsoft.Extensions.Logging;

namespace AlloyClient.Assets;

public static class AssetParser {
    public static async Task LoadAssetsAsync() {
        using var timer = new EasyTimer("Loading assets...", $"Loaded assets in {EasyTimer.Time}");
        await Task.WhenAll(Task.Run(ParseGround), Task.Run(ParseObjects));
    }

    private static void ParseGround() {
        var path = File.ReadAllText("Content/Xmls/Ground.xml");
        var groundContainer = XElement.Parse(path).Elements("Ground");
        Parallel.ForEach(groundContainer, ground => {
            var props = new GroundProperties(ground);
            lock (GroundLibrary.TypeToGroundProps)
                GroundLibrary.TypeToGroundProps.Add(props.ObjectType, props);
            
            lock (GroundLibrary.TypeToTextureData)
                GroundLibrary.TypeToTextureData.Add(props.ObjectType, new TextureData(ground));
            
            lock (GroundLibrary.IdToTileType)
                GroundLibrary.IdToTileType.Add(props.ObjectId, props.ObjectType);
        });
    }

    private static void ParseObjects() {
        var xmlFiles = Directory.GetFiles("Content/Xmls", "*.xml");
        Parallel.ForEach(xmlFiles, xmlFile => {
            var path = File.ReadAllText(xmlFile);
            var objectContainer = XElement.Parse(path).Elements("Object");
            Parallel.ForEach(objectContainer, gameObject => {
                var props = new ObjectProperties(gameObject);
                lock (ObjectLibrary.TypeToObjectProps)
                    ObjectLibrary.TypeToObjectProps.TryAdd(props.ObjectType, props);
                
                lock (ObjectLibrary.TypeToTextureData)
                    ObjectLibrary.TypeToTextureData.TryAdd(props.ObjectType, new TextureData(gameObject));
                
                lock (ObjectLibrary.IdToObjectType)
                    ObjectLibrary.IdToObjectType.TryAdd(props.ObjectId, props.ObjectType);

                if (props.Class == "Player") {
                    lock (ObjectLibrary.TypeToClassProps)
                        ObjectLibrary.TypeToClassProps.TryAdd(props.ObjectType, props.PlayerProperties);
                }

                if (props.Skin) {
                    lock (ObjectLibrary.TypeToSkins)
                        ObjectLibrary.TypeToSkins.Add(new Tuple<ushort, ushort>(props.PlayerClassType, props.ObjectType));
                }

                if (gameObject.HasElement("Item")) {
                    lock (ObjectLibrary.TypeToItem)
                        ObjectLibrary.TypeToItem.TryAdd(props.ObjectType, new ItemDesc(props.ObjectType, gameObject));
                }
            });
        });
    }
}

public sealed class TextureData {
    private static readonly ILogger Logger = Program.LogFactory.CreateLogger(nameof(TextureData));

    public bool HasAnimationData = false;
    
    public AtlasData Texture;
    public TextureData TopTexture;
    public AnimationAtlasData AnimatedTextures;
    public TextureData[] RandomTextures;
    public Dictionary<int, TextureData> AltTextures;
    public AtlasData? EditorTexture;
    public Color DominantColor = Color.Transparent;

    public TextureData EdgeTexture;
    public TextureData CornerTexture;
    public TextureData InnerCornerTexture;

    public TextureData(XElement xml) {
        if (xml.GetElement("Texture", out var elem)) {
            Texture = Main.Atlas.GetAtlasData(elem.GetValue<string>("File"), (int) elem.GetValue<uint>("Index"));
            DominantColor = Main.Atlas.GetDominantColor(elem.GetValue<string>("File"), elem.GetValue<int>("Index"));
        }

        if (xml.GetElement("Top", out elem)) {
            TopTexture = new TextureData(elem);
        }

        if (xml.GetElement("AnimatedTexture", out elem)) {
            AnimatedTextures = Main.Atlas.GetAnimationAtlasData(elem.GetValue<string>("File"), elem.GetValue<int>("Index"));
            HasAnimationData = true;
        }

        if (xml.GetElements("RandomTexture", out var elems)) {
            RandomTextures = new TextureData[elems.Length];
            for (var i = 0; i < elems.Length; i++) {
                RandomTextures[i] = new TextureData(elems[i]);
            }
        }

        if (xml.GetElements("AltTexture", out elems)) {
            AltTextures = [];
            foreach (var e in elems) {
                var id = e.GetAttribute("id", 0);
                if (AltTextures.ContainsKey(id)) {
                    Logger.Log(LogLevel.Warning, $"[Dupe AltTextureId] object: {xml.GetAttribute("id", "")} has dupe id: {id}");
                }

                AltTextures[id] = new TextureData(e);
            }
        }

        if (xml.GetElement("EditorTexture", out elem)) {
            EditorTexture = Main.Atlas.GetAtlasData(elem.GetValue<string>("File"), elem.GetValue<int>("Index"));
        }

        if (xml.GetElement("Edge", out elem)) {
            EdgeTexture = new TextureData(elem);
        }
        
        if (xml.GetElement("Corner", out elem)) {
            CornerTexture = new TextureData(elem);
        }
        
        if (xml.GetElement("InnerCorner", out elem)) {
            InnerCornerTexture = new TextureData(elem);
        }
    }

    public AtlasData GetTexture(out Color color, bool random = false, int id = 0) {
        if (RandomTextures == null) {
            color = DominantColor;
            return Texture;
        }

        if (random) {
            id = Random.Shared.Next(0, RandomTextures.Length);
        }

        return RandomTextures[id].GetTexture(out color, false, id);
    }

    public AtlasData GetTexture(bool random = false, int id = 0) {
        if (RandomTextures == null) {
            return Texture;
        }

        if (random) {
            id = Random.Shared.Next(0, RandomTextures.Length);
        }

        return RandomTextures[id].GetTexture(false, id);
    }

    public AtlasData GetTopTexture() {
        if (TopTexture == null) {
            return new AtlasData();
        }

        return TopTexture.GetTexture();
    }

    public bool GetAltTexture(int id, out TextureData data) {
        if (AltTextures == null || !AltTextures.ContainsKey(id)) {
            Logger.Log(LogLevel.Warning, $"[Missing AltTextureId] object: {id}");
            data = null;
            return false;
        }

        AltTextures.TryGetValue(id, out data);
        return true;
    }
}