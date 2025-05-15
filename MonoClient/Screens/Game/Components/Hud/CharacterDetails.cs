using MonoClient.Objects;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Data;
using MonoClient.UiLib.Enums;
using MonoClient.Utils;

namespace MonoClient.Screens.Game.Components.Hud;

public sealed class CharacterDetails : Sprite {
    
    private readonly ObjectRect _skin;
    private readonly SimpleText _name;

    public CharacterDetails() {
        _skin = new ObjectRect(new ObjectRectConfig { Width = 30, Height = 30 });
        _skin.Y = 4;
        _skin.X = 4;
        AddChild(_skin);
        _name = new SimpleText(new TextConfig {
            Text = "test",
            FontSize = 25,
            FontType = FontType.Bolder,
            X = 40,
            Y = 20,
            Color = 0xdadada,
            OutlineThickness = 0,
            OutlineColor = 0,
            Anchor = UiAnchor.MiddleLeft
        });
        AddChild(_name);
        
        Map.OnPlayerUpdate.Add(OnPlayerUpdate);
    }

    private void OnPlayerUpdate(Player player) {
        _name.SetText(player.Name);
        _skin.ChangeTexture(Atlas.Create(player.TextureData.AnimatedTextures.FaceRight[0], TextureType.GameAtlas));
    }
}