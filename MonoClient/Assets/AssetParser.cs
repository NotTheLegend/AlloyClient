using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Common;
using Common.Atlas;
using Microsoft.Xna.Framework;
using MonoClient.Assets.Libraries;
using MonoClient.Assets.XmlStructs;
using MonoClient.Objects.Util;
using MonoClient.Objects.Util.ItemDatas;
using MonoClient.Utils;

namespace MonoClient.Assets;

public static class AssetParser {
    public static async Task ParseAssetsAsync() {
        await ParseGround();
        await ParseObjects();
    }

    private static async Task ParseGround() {
        var path = await File.ReadAllTextAsync("Content/Xmls/Ground.xml");
        var groundContainer = XElement.Parse(path).Elements("Ground");
        foreach (var ground in groundContainer) {
            var props = new GroundProperties(ground);
            GroundLibrary.TypeToGroundProps[props.ObjectType] = props;
            GroundLibrary.TypeToTextureData[props.ObjectType] = new TextureData(ground);

            GroundLibrary.IdToTileType[props.ObjectId] = props.ObjectType;
        }
    }

    private static async Task ParseObjects() {
        var xmlFiles = Directory.GetFiles("Content/Xmls", "*.xml");
        var excludedFiles = new[]
            { "Ground.xml", "Particles.xml", "Regions.xml", "PotionStorageUpgradeItems.xml", "TutorialScript.xml" };

        foreach (var xmlFile in xmlFiles.Where(file => !excludedFiles.Contains(Path.GetFileName(file))).ToArray()) {
            var path = await File.ReadAllTextAsync(xmlFile);
            var objectContainer = XElement.Parse(path).Elements("Object");
            foreach (var gameObject in objectContainer) {
                var props = new ObjectProperties(gameObject);
                ObjectLibrary.TypeToObjectProps[props.ObjectType] = props;
                ObjectLibrary.TypeToTextureData[props.ObjectType] = new TextureData(gameObject);

                ObjectLibrary.IdToObjectType[props.ObjectId] = props.ObjectType;

                ObjectLibrary.IdToObjectType[props.ObjectId] = props.ObjectType;

                if (props.Class == "Player") {
                    ObjectLibrary.TypeToClassProps[props.ObjectType] = props.PlayerProperties;
                }

                if (gameObject.HasElement("Item")) {
                    ObjectLibrary.ItemXmls[props.ObjectType] = gameObject;
                }
            }
        }
    }
}

public sealed class TextureData {
    private static readonly Logger Log = new(typeof(TextureData));

    public bool HasAnimationData = false;
    
    public AtlasData Texture;
    public TextureData TopTexture;
    public AnimationAtlasData AnimatedTextures;
    public TextureData[] RandomTextures;
    public Dictionary<int, TextureData> AltTextures;
    public AtlasData? EditorTexture;
    public Color DominantColor = Color.Transparent;

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
                    Log.Warn($"[Dupe AltTextureId] object: {xml.GetAttribute("id", "")} has dupe id: {id}");
                }

                AltTextures[id] = new TextureData(e);
            }
        }

        if (xml.GetElement("EditorTexture", out elem)) {
            EditorTexture = Main.Atlas.GetAtlasData(elem.GetValue<string>("File"), elem.GetValue<int>("Index"));
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
            Log.Warn($"[Missing AltTextureId] object: {id}");
            data = null;
            return false;
        }

        AltTextures.TryGetValue(id, out data);
        return true;
    }
}