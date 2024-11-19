using System;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Title.Components.CharacterList;

public class ClassRect : Container {
    
    public int CharacterIndex;
    public ushort Type;
    
    public ClassRect(int characterIndex, ushort type) {
        CharacterIndex = characterIndex;
        Type = type;
        
        var frames = UiRender.GameAtlas.GetAnimationAtlasData("players", characterIndex);
        var charPortrait = new ObjectRect(new ObjectRectConfig {
            Texture = new TextureInfo(frames.FaceDown[0], TextureType.GameAtlas),
            Width = 160,
            Height = 160,
            Anchor = UiAnchor.Middle,
        });
        AddChild(charPortrait);
    }
}