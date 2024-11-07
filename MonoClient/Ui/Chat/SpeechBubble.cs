using System;
using Microsoft.Xna.Framework;
using MonoClient.Objects;
using MonoClient.State;
using MonoClient.UiLib;
using MonoClient.UiLib.BuiltIn;
using MonoClient.UiLib.Core;
using MonoClient.UiLib.Enums;

namespace MonoClient.Ui.Chat;

public struct SpeechData {

    public Entity Owner;

    public string Text;
}

public sealed class SpeechBubble : Sprite {

    private double _lifetime = 5000;
    
    private Entity _owner;
    private Sprite _parent;

    public SpeechBubble(SpeechData data, Sprite parent) {
        _owner = data.Owner;
        _parent = parent;

        const int cut = 10;
        
        var txt = new SimpleText(new TextConfig { Text = data.Text, FontSize = 15, X = cut, Y = cut, Color = 0, MaxWidth = 120 });

        var rect = new CutEdgeRect(new CutEdgeConfig { Width = cut * 2 + txt.Width, Height = cut * 2 + txt.Height, CutX = cut, CutY = cut, Color = 0xFFFFFF });
        AddChild(rect);

        
        AddChild(txt);
        SetAnchor(UiAnchor.MiddleBottom);
    }

    protected override void OnUpdate(GameTime gameTime) {
        if (_owner == null) return;
        if ((_lifetime -= gameTime.ElapsedGameTime.TotalMilliseconds) <= 0) _parent.RemoveChild(this);

        Scale = new Vector2(Settings.CameraZoom / 96f);
        
        var pos = Vector2.Transform(_owner.Position, Camera.SpeechMatrix);

        X = (int)pos.X;
        Y = (int)(pos.Y - _owner.SpeechOffset);
    }
}