using AlloyClient.Game;
using AlloyClient.Game.Objects;
using AlloyClient.State;
using AlloyClient.UiLib.BuiltIn;
using AlloyClient.UiLib.Core;
using AlloyClient.UiLib.Enums;
using OpenTK.Mathematics;
using AlloyClient.Utils;

namespace AlloyClient.Ui.Character;

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
        //AddEventListener(Event.EnterFrame, OnFrameEnter); todo: <---- (also broadcast is broken)
        AddEventListener(Event.AddedToStage, () => { AddEventListener(Event.EnterFrame, OnFrameEnter); });
        AddEventListener(Event.RemovedFromStage, () => { RemoveEventListener(Event.EnterFrame, OnFrameEnter); });
}
    
    private void OnFrameEnter() {
        if (_owner == null) return;

        var gameTime = Stage.GameTime;
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