using AlloyClient.State;
using AlloyClient.UiLib.BuiltIn.Buttons;
using AlloyClient.UiLib.Core;
using AlloyClient.UiLib.Data;
using AlloyClient.UiLib.Enums;
using AlloyClient.Utils;

namespace AlloyClient.Ui.Components.Buttons;

public struct MusicButtonConfig {
    
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.LeftTop;
    
    public MusicButtonConfig() { }
}

public class MusicButton : Sprite {

    private readonly TextureInfo _musicOn;
    private readonly TextureInfo _musicOff;

    private readonly IconButton _button;

    private bool _state;
    
    //TODO: turn into its own thing instead of having icon child

    public MusicButton(MusicButtonConfig config) {
        _state = Settings.PlayMusic.Value;
        _musicOn = TextureHelper.FromGameAtlas("lofiInterfaceBig", 3, 4);
        _musicOff = TextureHelper.FromGameAtlas("lofiInterfaceBig", 4, 4);

        var iconConfig = new IconButtonConfig { Texture = _state ? _musicOn : _musicOff, Width = config.Width, Height = config.Height, OnClick = OnClick, Alpha = config.Alpha, Anchor = config.Anchor, GameObjectShade = false };
        _button = new IconButton(iconConfig);
        AddChild(_button);

        X = config.X;
        Y = config.Y;
        MouseEnabled = true;
        AddEventListener(MouseEvent.MouseOver, OnMouseOver);
        AddEventListener(MouseEvent.MouseOut, OnMouseOut);
    }

    private void OnClick() {
        _state = !_state;
        Settings.PlayMusic = _state;
        //TODO: add mute
        
        _button.ChangeTexture(_state ? _musicOn : _musicOff);
    }

    private void OnMouseOver() => _button.SetColor(0xFFDC85);
    
    private void OnMouseOut() => _button.SetColor(0xFFFFFF);
    
}