using Common;
using MonoClient.Objects;
using MonoClient.State;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;
using MonoClient.Utils;
using OpenTK.Mathematics;
using MathUtils = MonoClient.Utils.MathUtils;

namespace MonoClient.Ui.Character;

public class CharacterStatusText : Sprite {
    private const int MaxDrift = 20;
    
    private Entity _owner;

    private string _text;
    private uint _color;
    private double _lifetime;
    private int _offsetTime;

    private double _startTime;
    

    public CharacterStatusText(Entity en, string text, uint color, int lifetime, int offsetTime) {
        _owner = en;

        _text = text;
        _color = color;
        _lifetime = lifetime;
        _offsetTime = offsetTime;

        var txtConfig = new TextConfig {
            Text = _text,
            Color = _color,
            MaxWidth = 120,
            FontSize = 20,
            OutlineThickness = 4
        };
        var txt = new SimpleText(txtConfig);
        AddChild(txt);
        
        SetAnchor(UiAnchor.MiddleBottom);
    }
    
    protected override void OnUpdate(GameTime gameTime) {
        if (_owner == null) return;

        var currentTime = gameTime.TotalMs;
        
        if (_startTime == 0) {
            _startTime = currentTime + _offsetTime;
        }
        
        if (currentTime < _startTime) return;
        
        var elapsedTime = currentTime - _startTime;
        if (elapsedTime >= _lifetime) {
            Parent.RemoveChild(this);
            return;
        }
        
        Scale = new Vector2(Settings.CameraZoom / 96f);
        
        var w = Camera.VisibleTileRadius.X;
        var h = Camera.VisibleTileRadius.Y;

        var x = MathUtils.Map(_owner.Position.X - Camera.Position.X, -w, w, 0f, Settings.ScreenWidth - Camera.HudOffset);
        var y = MathUtils.Map(_owner.Position.Y + _owner.HeightOffset + Camera.Position.Y, -h, h, 0f, Settings.ScreenHeight);
        
        var drift = elapsedTime / _lifetime * MaxDrift;

        X = (int)x;
        Y = (int)(y - drift);
        
        var remainingLifetime = _lifetime - elapsedTime;
        Alpha = (float)(remainingLifetime / _lifetime);
    }
}