using System;
using Common;
using RealmClient.UiLib.BuiltIn;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;
using OpenTK.Mathematics;
using RealmClient.Game;
using RealmClient.Game.Objects;
using RealmClient.State;
using RealmClient.Utils;
using MathUtils = RealmClient.Utils.MathUtils;

namespace RealmClient.Ui.Chat;

public record struct SpeechData(Entity Owner, string Text, string Recipient);

public sealed class SpeechBubble : Sprite {

    private double _lifetime = 5000;
    
    private readonly Entity _owner;

    public SpeechBubble(SpeechData data) {
        _owner = data.Owner;
        var type = SpeechColors.Default;
        
        switch (data.Recipient) {
            case "*Guild*":
                type = SpeechColors.Guild;
                break;
            case "*Party*":
                type = SpeechColors.Party;
                break;
            default: {
                if (!string.IsNullOrEmpty(data.Recipient)) {
                    type = SpeechColors.Tell;
                    break;
                }

                if (_owner.Properties.IsEnemy)
                    type = SpeechColors.Enemy; 
                break;
            }
        }

        // colors = (background, outline, text)
        var colors = type switch {
            SpeechColors.Enemy => (0x561F1Cu, 0xFC8642u, 0xCDC0BFu),
            SpeechColors.Tell => (0x260AB6u, 0x00F0FFu, 0xD3CCF7u),
            SpeechColors.Guild => (0x3E8A00u, 0xA6FF5Du, 0xD3F7CCu),
            SpeechColors.Party => (0x6428AFu, 0xB478FFu, 0xF0C8E6u),
            _ => (0xE1DFDCu, 0xFFFFFFu, 0x545454u)
        };
        
        const int cut = 10;
        const int outline = 2;
        
        var txt = new SimpleText(new TextConfig { Text = data.Text, FontSize = 15, X = cut, Y = cut, Color = colors.Item3, MaxWidth = 120 });
        var rect = new CutEdgeRect(new CutEdgeConfig { Width = cut * 2 + txt.Width, Height = cut * 2 + txt.Height, CutX = cut, CutY = cut, Color = colors.Item1 });
        var outlineRect = new CutEdgeRect(new CutEdgeConfig { Width = (cut + outline) * 2 + txt.Width, Height = (cut + outline) * 2 + txt.Height, CutX = cut + outline / 2, CutY = cut + outline / 2, Color = colors.Item2 }) {
            X = -outline,
            Y = -outline
        };
        
        AddChild(outlineRect);
        AddChild(rect);
        AddChild(txt);
        
        SetAnchor(UiAnchor.MiddleBottom);
        AddEventListener(Event.EnterFrame, OnFrameEnter);
    }

    private void OnFrameEnter() {
        var gameTime = Stage.GameTime;
        if (_owner == null) return;
        if ((_lifetime -= gameTime.ElapsedMs) <= 0) Parent.RemoveChild(this);

        Scale = new Vector2(Settings.CameraZoom / 96f);
        
        var w = Camera.VisibleTileRadius.X;
        var h = Camera.VisibleTileRadius.Y;
        
        var s = MathF.Sin(-Settings.CameraAngle);
        var c = MathF.Cos(-Settings.CameraAngle);

        var x = _owner.Position.X - Camera.Position.X;
        var y = _owner.Position.Y + Camera.Position.Y;
            
        var x1 = x * c - y * s;
        var y1 = x * s + y * c;

        var newX = MathUtils.Map(x1, -w, w, 0f, Settings.ScreenWidth - Camera.HudOffset);
        var newY = MathUtils.Map(y1, -h, h, 0f, Settings.ScreenHeight);
        var offset = _owner.HeightOffset / (h + h) * Settings.ScreenHeight;

        X = (int)newX;
        Y = (int)newY + (int)offset;
    }
}