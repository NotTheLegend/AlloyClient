using Alloy.Engine;
using AlloyClient.Game;
using AlloyClient.Game.Objects;
using Alloy.UiLib.BuiltIn;
using Alloy.UiLib.Core;
using OpenTK.Mathematics;

namespace AlloyClient.Ui.Chat;

public record struct SpeechData(Entity Owner, string Text, string Recipient);

public sealed class SpeechBubble : Sprite {

    private const double DefaultLifetimeMs = 5000;

    private readonly double _lifetime;
    private readonly Entity _owner;

    public SpeechBubble(SpeechData data, double time) {
        _lifetime = time + DefaultLifetimeMs;
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
    }

    public bool Update(in GameTime gameTime, in Camera camera) {
        if (_lifetime < gameTime.TotalMs || _owner == null) {
            return false;
        }
        
        var pos = camera.WorldToScreen(new Vector3(_owner.X, _owner.Y, _owner.Z - _owner.HeightOffset), Stage.Dimensions);
        
        Scale = new Vector2(Settings.CameraZoom);
        X = pos.X;
        Y = pos.Y;
        return true;
    }
}