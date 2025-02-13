using MonoClient.Objects;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Screens.Game.Components.Hud;

public sealed class CharacterDetails : Sprite {
    
    private readonly ObjectRect _skin;
    private readonly SimpleText _name;

    public CharacterDetails() {
        _skin = new ObjectRect(new ObjectRectConfig { Width = 40, Height = 40 });
        AddChild(_skin);
        _name = new SimpleText(new TextConfig {
            Text = "test",
            FontSize = 25,
            Bold = 2,
            X = 44,
            Y = 20, 
            OutlineThickness = 4,
            Color = 0xb3b3b3,
            OutlineColor = 0,
            Anchor = UiAnchor.MiddleLeft
        });
        AddChild(_name);
        
        Map.OnPlayerUpdate.Add(OnPlayerUpdate);
    }

    private void OnPlayerUpdate(Player player) {
        _name.SetText(player.Name);
        _skin.ChangeTexture(new TextureInfo(player.TextureData.AnimatedTextures.FaceRight[0], TextureType.GameAtlas));
    }
}