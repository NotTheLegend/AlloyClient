using Alloy.UiLib.BuiltIn;

namespace AlloyClient.Ui.Components.Graphics;

/// <summary>
/// Shared title-screen ribbon, positioned to match the Haxe ScreenGraphic.
/// </summary>
public sealed class TitleMenuRibbon : Container {
    public const int TopY = 629;
    public const int RibbonHeight = 62;
    public const int MenuCenterY = TopY + RibbonHeight / 2;

    private const int FadeWidth = 120;
    private const int FadeSteps = 32;

    private readonly ColorRect _body;
    private readonly ColorRect _bottomEdge;
    private readonly ColorRect[] _leftBodyFade = new ColorRect[FadeSteps];
    private readonly ColorRect[] _rightBodyFade = new ColorRect[FadeSteps];
    private readonly ColorRect[] _leftEdgeFade = new ColorRect[FadeSteps];
    private readonly ColorRect[] _rightEdgeFade = new ColorRect[FadeSteps];

    public TitleMenuRibbon(int width)
        : base(new ContainerConfig { Width = width, Height = RibbonHeight }) {
        _body = new ColorRect(new ColorRectConfig {
            Height = RibbonHeight - 1,
            Color = 0x545454
        });
        AddChild(_body);

        _bottomEdge = new ColorRect(new ColorRectConfig {
            Y = RibbonHeight - 1,
            Height = 1,
            Color = 0x4C4C4C
        });
        AddChild(_bottomEdge);

        for (var i = 0; i < FadeSteps; i++) {
            var alpha = i / (FadeSteps - 1f);
            _leftBodyFade[i] = AddFadeSegment(0x545454, RibbonHeight - 1, 0, alpha);
            _rightBodyFade[i] = AddFadeSegment(0x545454, RibbonHeight - 1, 0, alpha);
            _leftEdgeFade[i] = AddFadeSegment(0x4C4C4C, 1, RibbonHeight - 1, alpha);
            _rightEdgeFade[i] = AddFadeSegment(0x4C4C4C, 1, RibbonHeight - 1, alpha);
        }

        Layout(width);
    }

    public void ResizeWidth(int width) {
        Resize(width, RibbonHeight);
        Layout(width);
    }

    private ColorRect AddFadeSegment(uint color, int height, int y, float alpha) {
        var segment = new ColorRect(new ColorRectConfig {
            Y = y,
            Height = height,
            Color = color,
            Alpha = alpha
        });
        AddChild(segment);
        return segment;
    }

    private void Layout(int width) {
        var fadeWidth = System.Math.Min(FadeWidth, width / 2);
        _body.X = fadeWidth;
        _body.Resize(System.Math.Max(0, width - fadeWidth * 2), RibbonHeight - 1);
        _bottomEdge.X = fadeWidth;
        _bottomEdge.Resize(System.Math.Max(0, width - fadeWidth * 2), 1);

        for (var i = 0; i < FadeSteps; i++) {
            var x0 = i * fadeWidth / FadeSteps;
            var x1 = (i + 1) * fadeWidth / FadeSteps;
            var segmentWidth = x1 - x0;

            PositionSegment(_leftBodyFade[i], x0, segmentWidth, RibbonHeight - 1);
            PositionSegment(_leftEdgeFade[i], x0, segmentWidth, 1);

            var rightX = width - x1;
            PositionSegment(_rightBodyFade[i], rightX, segmentWidth, RibbonHeight - 1);
            PositionSegment(_rightEdgeFade[i], rightX, segmentWidth, 1);
        }
    }

    private static void PositionSegment(ColorRect segment, int x, int width, int height) {
        segment.X = x;
        segment.Resize(width, height);
    }
}
