using Alloy.Common;
using Alloy.UiLib.Core;
using OpenTK.Mathematics;

namespace Alloy.UiLib.BuiltIn;

public struct CutEdgeConfig {
    public int X = 0;
    public int Y = 0;
    public int Width = 0;
    public int Height = 0;
    public int CutX = 0;
    public int CutY = 0;
    public CutEdges Cuts = CutEdges.All;
    public Color Color = Color.Black;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.None;

    public bool MouseEnabled = false;

    public CutEdgeConfig() { }
}

public sealed class CutEdgeRect : Sprite {

    private readonly int _cx;
    private readonly int _cy;
    private readonly CutEdges _cuts;

    public CutEdgeRect(CutEdgeConfig config) {
        X = config.X;
        Y = config.Y;
        _cx = config.CutX;
        _cy = config.CutY;
        _cuts = config.Cuts;
        Alpha = config.Alpha;
        Anchor = config.Anchor;
        MouseEnabled = config.MouseEnabled;
        
        TextureId = TextureType.SolidColor;
        HitboxType = CollisionType.Square;
        
        Build(config.Width, config.Height);
        SetColor(config.Color);
    }

    private void Build(int w, int h) {
        var data = GetGraphicsBuffer(54);
        
        // Top Left
        data[0].Position =(_cuts & CutEdges.TopLeft) != 0 ? new Vector2(_cx / 2f, _cy / 2f) : new Vector2(0f, 0f);
        data[1].Position = new Vector2(_cx, 0f);
        data[2].Position = new Vector2(_cx, _cy);
        data[3].Position = (_cuts & CutEdges.TopLeft) != 0 ? new Vector2(_cx / 2f, _cy / 2f) : new Vector2(0f, 0f);
        data[4].Position = new Vector2(_cx, _cy);
        data[5].Position = new Vector2(0f, _cy);
        // Top Center
        data[6].Position = new Vector2(_cx, 0);
        data[7].Position = new Vector2(w - _cx, 0);
        data[8].Position = new Vector2(w - _cx, _cy);
        data[9].Position = new Vector2(_cx, 0);
        data[10].Position = new Vector2(w - _cx, _cy);
        data[11].Position = new Vector2(_cx, _cy);
        // Top Right
        data[12].Position = (_cuts & CutEdges.TopRight) != 0 ? new Vector2(w - _cx / 2f, _cy / 2f) : new Vector2(w, 0f);
        data[13].Position = new Vector2(w, _cy);
        data[14].Position = new Vector2(w - _cx, _cy);
        data[15].Position = (_cuts & CutEdges.TopRight) != 0 ? new Vector2(w - _cx / 2f, _cy / 2f) : new Vector2(w, 0f);
        data[16].Position = new Vector2(w - _cx, _cy);
        data[17].Position = new Vector2(w - _cx, 0f);
        // Middle Left
        data[18].Position = new Vector2(0, _cy);
        data[19].Position = new Vector2(_cx, _cy);
        data[20].Position = new Vector2(_cx, h - _cy);
        data[21].Position = new Vector2(0, _cy);
        data[22].Position = new Vector2(_cx, h - _cy);
        data[23].Position = new Vector2(0, h - _cy);
        // Middle
        data[24].Position = new Vector2(_cx, _cy);
        data[25].Position = new Vector2(w - _cx, _cy);
        data[26].Position = new Vector2(w - _cx, h - _cy);
        data[27].Position = new Vector2(_cx, _cy);
        data[28].Position = new Vector2(w - _cx, h - _cy);
        data[29].Position = new Vector2(_cx, h - _cy);
        // Middle Right
        data[30].Position = new Vector2(w - _cx, _cy);
        data[31].Position = new Vector2(w, _cy);
        data[32].Position = new Vector2(w, h - _cy);
        data[33].Position = new Vector2(w - _cx, _cy);
        data[34].Position = new Vector2(w, h - _cy);
        data[35].Position = new Vector2(w - _cx, h - _cy);
        // Bottom Left
        data[36].Position = (_cuts & CutEdges.BottomLeft) != 0 ? new Vector2(_cx / 2f, h - _cy / 2f) : new Vector2(0f, h);
        data[37].Position = new Vector2(0, h - _cy);
        data[38].Position = new Vector2(_cx, h - _cy);
        data[39].Position = (_cuts & CutEdges.BottomLeft) != 0 ? new Vector2(_cx / 2f, h - _cy / 2f) : new Vector2(0f, h);
        data[40].Position = new Vector2(_cx, h - _cy);
        data[41].Position = new Vector2(_cx, h);
        // Bottom Middle
        data[42].Position = new Vector2(_cx, h - _cy);
        data[43].Position = new Vector2(w - _cx, h - _cy);
        data[44].Position = new Vector2(w - _cx, h);
        data[45].Position = new Vector2(_cx, h - _cy);
        data[46].Position = new Vector2(w - _cx, h);
        data[47].Position = new Vector2(_cx, h);
        // Bottom Right
        data[48].Position = (_cuts & CutEdges.BottomRight) != 0 ? new Vector2(w - _cx / 2f, h - _cy / 2f) : new Vector2(w, h);
        data[49].Position = new Vector2(w - _cx, h);
        data[50].Position = new Vector2(w - _cx, h - _cy);
        data[51].Position = (_cuts & CutEdges.BottomRight) != 0 ? new Vector2(w - _cx / 2f, h - _cy / 2f) : new Vector2(w, h);
        data[52].Position = new Vector2(w - _cx, h - _cy);
        data[53].Position = new Vector2(w, h - _cy);
        
        SetGraphicsBuffer();
    }

    public void Resize(int width, int height) => Build(width, height);

    public void SetColor(Color color) => SetColorChannel1(color);
}