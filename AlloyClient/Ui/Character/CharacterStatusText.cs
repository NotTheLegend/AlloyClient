using System;
using Alloy.Engine;
using AlloyClient.Game;
using AlloyClient.Game.Objects;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using OpenTK.Mathematics;
using AlloyClient.Utils;

namespace AlloyClient.Ui.Character;

public class CharacterStatusText : Sprite {
    private const int MaxDrift = 20;

    private readonly Entity _owner;
    private readonly double _lifetime;
    private readonly double _endTime;
    private readonly double _startTime;
    
    public CharacterStatusText(Entity en, string text, uint color, double lifetime, double startTime) {
        _owner = en;
        _lifetime = lifetime;
        _endTime = startTime + lifetime;
        _startTime = startTime;

        var txtConfig = new TextConfig {
            Text = text,
            Color = color,
            MaxWidth = 120,
            FontSize = 20,
            OutlineThickness = 4
        };
        
        AddChild(new SimpleText(txtConfig));

        Visible = false;
        Anchor = UiAnchor.MiddleBottom;
}
    
    public bool Update(in GameTime gameTime, in Camera camera) {
        if (_owner == null || _endTime < gameTime.TotalMs) {
            return false;
        }

        Visible = _startTime < gameTime.TotalMs;
        Scale = new Vector2(Settings.CameraZoom);
        
        var pos = camera.WorldToScreen(new Vector3(_owner.X, _owner.Y, _owner.Z - _owner.HeightOffset), Stage.Dimensions);
        var elapsed = (gameTime.TotalMs - _startTime) / _lifetime;
        var drift = elapsed / (camera.VisibleTileRadius.Y * 2) * Stage.StageHeight;

        X = pos.X;
        Y = pos.Y - (int)drift;
        Alpha = (float)(1 - elapsed);
        return true;
    }
}