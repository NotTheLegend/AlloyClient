using System;
using RealmClient.UiLib;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Data;
using RealmClient.UiLib.Enums;
using RealmClient.Utils;

namespace RealmClient.Screens.Title.Components.CharacterList;

public class ClassRect : Container {
    
    public int CharacterIndex;
    public ushort Type;
    
    public ClassRect(int characterIndex, ushort type) {
        CharacterIndex = characterIndex;
        Type = type;
        
        var frames = Main.Atlas.GetAnimationAtlasData("players", characterIndex);
        var charPortrait = new ObjectRect(new ObjectRectConfig {
            Texture = TextureHelper.Create(frames.FaceDown[0], TextureType.GameAtlas),
            Width = 160,
            Height = 160,
            Anchor = UiAnchor.Middle,
        });
        AddChild(charPortrait);
    }
}