using Alloy.UiLib.BuiltIn;

namespace AlloyClient.Ui.Components.Graphics;

public sealed class CutCornerOutline : Container {
    public CutCornerOutline(int width, int height, uint color = 0xFFFFFF, float alpha = 0.35f)
        : base(new ContainerConfig { Width = width, Height = height }) {
        AddLine(3, 0, width - 6, 1, color, alpha);
        AddLine(3, height - 1, width - 6, 1, color, alpha);
        AddLine(0, 3, 1, height - 6, color, alpha);
        AddLine(width - 1, 3, 1, height - 6, color, alpha);

        AddLine(2, 1, 1, 1, color, alpha);
        AddLine(1, 2, 1, 1, color, alpha);
        AddLine(width - 3, 1, 1, 1, color, alpha);
        AddLine(width - 2, 2, 1, 1, color, alpha);
        AddLine(1, height - 3, 1, 1, color, alpha);
        AddLine(2, height - 2, 1, 1, color, alpha);
        AddLine(width - 2, height - 3, 1, 1, color, alpha);
        AddLine(width - 3, height - 2, 1, 1, color, alpha);
    }

    private void AddLine(int x, int y, int width, int height, uint color, float alpha) {
        AddChild(new ColorRect(new ColorRectConfig {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Color = color,
            Alpha = alpha
        }));
    }
}
