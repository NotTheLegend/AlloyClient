using MonoClient.Sound;
using MonoClient.State;
using MonoClient.UiLib.BuiltIn.Buttons;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Core.Events.Types;
using MonoClient.UiLib.Enums;

namespace MonoClient.Ui.Components.Buttons;

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
        _musicOn = TextureInfo.FromGameAtlas("lofiInterfaceBig", 3);
        _musicOff = TextureInfo.FromGameAtlas("lofiInterfaceBig", 4);

        var iconConfig = new IconButtonConfig { Texture = _state ? _musicOn : _musicOff, Width = config.Width, Height = config.Height, OnClick = OnClick };
        _button = new IconButton(iconConfig);
        AddChild(_button);
        
        SetBaseDimensions(_button.Width, _button.Height);
        MouseEnabled = true;
        AddEventListener(MouseEventId.MouseOver, OnMouseOver);
        AddEventListener(MouseEventId.MouseOut, OnMouseOut);
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