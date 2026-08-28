using System;
using System.Collections.Generic;
using Alloy.Common;
using AlloyClient.Game;
using AlloyClient.Game.Objects;
using AlloyClient.Rendering.VertexData;
using OpenTK.Mathematics;

namespace AlloyClient.Rendering.Types.SubTypes;

public class TypeBar : SubRenderBase {
    public const float HalfWidthPixels = 20f;
    public const float HeightPixels = 5f;
    public const float BackgroundOffsetPixels = 1.2f;
    public const float BaseYOffsetPixels = 8f;
    public const float OtherPlayerYOffsetPixels = 16f;
    public const float RowSpacingPixels = HeightPixels + 1f;

    public override float Height => RowSpacingPixels;

    private readonly Color _backgroundColor = Color.FromHexRGB(0x111111);
    private Vector4 _backgroundScale;
    private float _fill;

    public TypeBar(RenderBase parent, Entity entity, Color color) {
        Parent = parent;
        Entity = entity;
        Color = color;

        UV = Vector4.Zero;
        _backgroundScale = Vector4.Zero;
        Rotation = new Vector4(0, 1, 1, -1);
        Extra = new ExtraData(RenderConfig.TypeBar, RenderConfig.NoShade);
    }

    public void SetFill(float percent) {
        _fill = Math.Clamp(percent, 0f, 1f);
    }

    public override void Draw(float yOffset, List<VertexObject> targets, double time) {
        var pixelScale = 1f / (Camera.BaseCameraZoom * Settings.CameraZoom);
        var halfWidth = HalfWidthPixels * pixelScale;
        var halfHeight = HeightPixels * 0.5f * pixelScale;
        var backgroundOffset = BackgroundOffsetPixels * pixelScale;
        var centerY = (yOffset + HeightPixels * 0.5f) * pixelScale;

        _backgroundScale.X = (halfWidth + backgroundOffset) * 2f;
        _backgroundScale.Y = (halfHeight + backgroundOffset) * 2f;
        _backgroundScale.Z = 0f;
        _backgroundScale.W = centerY;
        var backgroundExtra = Extra;
        backgroundExtra.SortId += 0.000001f;
        targets.Add(new VertexObject(Parent.Position, UV, _backgroundScale, Rotation, backgroundExtra, _backgroundColor));

        if (_fill <= 0f) {
            return;
        }

        Scale.X = halfWidth * 2f * _fill;
        Scale.Y = halfHeight * 2f;
        Scale.Z = halfWidth * (_fill - 1f);
        Scale.W = centerY;
        targets.Add(new VertexObject(Parent.Position, UV, Scale, Rotation, Extra, Color));
    }
}
