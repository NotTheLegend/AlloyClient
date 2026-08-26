using System;
using AlloyClient.Assets.Libraries;
using AlloyClient.Game.Objects.Enums;
using Alloy.UiLib.Data;
using Alloy.Common.Structs;
using Alloy.UiLib.Core;
using OpenTK.Mathematics;

namespace AlloyClient.Utils;

public static class TextureHelper {
    public static TextureInfo FromUiAtlas(string lookup, int index = 0, bool padding = true) {
        var uv = Main.UiAtlas.GetAtlasData(lookup, index);
        if (!padding) uv.RemovePadding();
        return new TextureInfo(uv.ToPosition(), TextureType.UiAtlas);
    }
    
    public static TextureInfo FromGameAtlas(string lookup, int index, bool padding = true) {
        var uv = Main.Atlas.GetAtlasData(lookup, index);
        if (!padding) uv.RemovePadding();
        return new TextureInfo(uv.ToPosition(), TextureType.GameAtlas);
    }
    
    public static TextureInfo FromGameAtlas(string lookup, int index, uint removePadding) {
        var uv = Main.Atlas.GetAtlasData(lookup, index);
        uv.RemovePadding(removePadding);
        return new TextureInfo(uv.ToPosition(), TextureType.GameAtlas);
    }
    
    public static TextureInfo FromGameAtlas(ushort id) {
        if (!ObjectLibrary.TypeToTextureData.TryGetValue(id, out var data))
            return FromGameAtlas("invisible", 0);
        var uv = data.GetTexture();
        return new TextureInfo(uv.ToPosition(), TextureType.GameAtlas);
    }

    public static TextureInfo Create(AtlasData uv, TextureType type, bool padding = true) {
        if (!padding) uv.RemovePadding();
        return new TextureInfo(uv.ToPosition(), type);
    }

    public static AtlasPosition ToPosition(this AtlasData data) {
        return new AtlasPosition(data.U, data.V, data.W, data.H);
    }

    extension(AnimationAtlasData data) {

        public AtlasData TextureFromFacing(float angle, AnimationType action, float idx, out bool attackFrame, out bool flipped) {
            var ca = MathUtils.BoundToPi(angle - Settings.CameraAngle);
            var sec = (int) (ca / MathHelper.PiOver4 + 4) % 8;

            var frames = sec switch {
                0 or 7 => data.FaceRight,
                1 or 2 => data.FaceUp,
                3 or 4 => data.FaceRight,
                5 or 6 => data.FaceDown,
                _ => data.FaceRight
            };

            flipped = sec switch {
                0 or 7 => true,
                _ => false
            };
            
            idx = Math.Max(0, Math.Min(0.99999f, idx));
            var i = action switch {
                AnimationType.Attack => 4 + (int) (idx * 2),
                AnimationType.Walk => 1 + (int) (idx * 2),
                _ => 0
            };

            attackFrame = i == 5;
            return frames[i];
        }
    }
}