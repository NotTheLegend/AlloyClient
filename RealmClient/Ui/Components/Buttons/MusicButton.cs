using RealmClient.UiLib.BuiltIn.Buttons;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Data;
using RealmClient.UiLib.Enums;
using RealmClient.Sound;
using RealmClient.State;
using RealmClient.Utils;

namespace RealmClient.Ui.Components.Buttons;

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

    public MusicButton(MusicButtonConfig config) {
        _state = Settings.PlayMusic.Value;
        _musicOn = TextureHelper.FromGameAtlas("lofiInterfaceBig", 3);
        _musicOff = TextureHelper.FromGameAtlas("lofiInterfaceBig", 4);

        var iconConfig = new IconButtonConfig { Texture = _state ? _musicOn : _musicOff, Width = config.Width, Height = config.Height, OnClick = OnClick };
        _button = new IconButton(iconConfig);
        AddChild(_button);
        
        SetBaseDimensions(_button.Width, _button.Height);
        MouseEnabled = true;
        AddEventListener(MouseEvent.MouseOver, OnMouseOver);
        AddEventListener(MouseEvent.MouseOut, OnMouseOut);
    }

    private void OnClick() {
        _state = !_state;
        Settings.PlayMusic = _state;
        Music.ToggleMusic(_state);
        
        _button.ChangeTexture(_state ? _musicOn : _musicOff);
    }

    private void OnMouseOver() => _button.SetColor(0xFFDC85);
    
    private void OnMouseOut() => _button.SetColor(0xFFFFFF);
    
}