using Alloy.UiLib.Core;
using Alloy.UiLib.Rendering;
using OpenTK.Mathematics;

namespace AlloyClient.Editor.Ui;

internal sealed class EditorOutlineRect : Sprite {
    public EditorOutlineRect(int x, int y, int width, int height, uint color, float alpha, int thickness = 1) {
        X = x;
        Y = y;
        width = System.Math.Max(thickness * 2, width);
        height = System.Math.Max(thickness * 2, height);
        SetColor(color);
        Alpha = alpha;
        TextureId = TextureType.Color;

        var right = (float)width;
        var bottom = (float)height;
        var inset = (float)thickness;
        VertexData = [
            new VertexUi(new Vector2(0, 0)),
            new VertexUi(new Vector2(right, 0)),
            new VertexUi(new Vector2(right, bottom)),
            new VertexUi(new Vector2(0, bottom)),
            new VertexUi(new Vector2(inset, inset)),
            new VertexUi(new Vector2(right - inset, inset)),
            new VertexUi(new Vector2(right - inset, bottom - inset)),
            new VertexUi(new Vector2(inset, bottom - inset))
        ];
        
        Indices = [
            0, 1, 5, 0, 5, 4,
            1, 2, 6, 1, 6, 5,
            2, 3, 7, 2, 7, 6,
            3, 0, 4, 3, 4, 7
        ];
        
        SetGraphicsBuffer();
    }
}
