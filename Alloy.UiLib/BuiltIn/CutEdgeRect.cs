using Alloy.UiLib.Core;
using Alloy.UiLib.Rendering;
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
    public uint Color = 0x000000;
    public float Alpha = 1.0f;
    public UiAnchor Anchor = UiAnchor.LeftTop;

    public bool MouseEnabled = false;

    public CutEdgeConfig() { }
}

public sealed class CutEdgeRect : Sprite {
       
    private int _w;
    private int _h;

    private int _cx;
    private int _cy;
    private CutEdges _cuts;

    public CutEdgeRect(CutEdgeConfig config) {
        X = config.X;
        Y = config.Y;
        _w = config.Width;
        _h = config.Height;
        _cx = config.CutX;
        _cy = config.CutY;
        _cuts = config.Cuts;
        SetColor(config.Color);
        Alpha = config.Alpha;
        Anchor = config.Anchor;
        MouseEnabled = config.MouseEnabled;
        
        TextureId = TextureType.Color;
        
        SetHitboxType(CollisionType.Vertices);

        EnsureBufferCapacity(54);
        FillData();
    }

    private void FillData() {
        // Top Left
        VertexData[0] = new VertexUi((_cuts & CutEdges.TopLeft) != 0 ? new Vector2(_cx / 2f, _cy / 2f) : new Vector2(0f, 0f));
        VertexData[1] = new VertexUi(new Vector2(_cx, 0f));
        VertexData[2] = new VertexUi(new Vector2(_cx, _cy));
        VertexData[3] = new VertexUi((_cuts & CutEdges.TopLeft) != 0 ? new Vector2(_cx / 2f, _cy / 2f) : new Vector2(0f, 0f));
        VertexData[4] = new VertexUi(new Vector2(_cx, _cy));
        VertexData[5] = new VertexUi(new Vector2(0f, _cy));
        // Top Center
        VertexData[6] = new VertexUi(new Vector2(_cx, 0));
        VertexData[7] = new VertexUi(new Vector2(_w - _cx, 0));
        VertexData[8] = new VertexUi(new Vector2(_w - _cx, _cy));
        VertexData[9] = new VertexUi(new Vector2(_cx, 0));
        VertexData[10] = new VertexUi(new Vector2(_w - _cx, _cy));
        VertexData[11] = new VertexUi(new Vector2(_cx, _cy));
        // Top Right
        VertexData[12] = new VertexUi((_cuts & CutEdges.TopRight) != 0 ? new Vector2(_w - _cx / 2f, _cy / 2f) : new Vector2(_w, 0f));
        VertexData[13] = new VertexUi(new Vector2(_w, _cy));
        VertexData[14] = new VertexUi(new Vector2(_w - _cx, _cy));
        VertexData[15] = new VertexUi((_cuts & CutEdges.TopRight) != 0 ? new Vector2(_w - _cx / 2f, _cy / 2f) : new Vector2(_w, 0f));
        VertexData[16] = new VertexUi(new Vector2(_w - _cx, _cy));
        VertexData[17] = new VertexUi(new Vector2(_w - _cx, 0f));
        // Middle Left
        VertexData[18] = new VertexUi(new Vector2(0, _cy));
        VertexData[19] = new VertexUi(new Vector2(_cx, _cy));
        VertexData[20] = new VertexUi(new Vector2(_cx, _h - _cy));
        VertexData[21] = new VertexUi(new Vector2(0, _cy));
        VertexData[22] = new VertexUi(new Vector2(_cx, _h - _cy));
        VertexData[23] = new VertexUi(new Vector2(0, _h - _cy));
        // Middle
        VertexData[24] = new VertexUi(new Vector2(_cx, _cy));
        VertexData[25] = new VertexUi(new Vector2(_w - _cx, _cy));
        VertexData[26] = new VertexUi(new Vector2(_w - _cx, _h - _cy));
        VertexData[27] = new VertexUi(new Vector2(_cx, _cy));
        VertexData[28] = new VertexUi(new Vector2(_w - _cx, _h - _cy));
        VertexData[29] = new VertexUi(new Vector2(_cx, _h - _cy));
        // Middle Right
        VertexData[30] = new VertexUi(new Vector2(_w - _cx, _cy));
        VertexData[31] = new VertexUi(new Vector2(_w, _cy));
        VertexData[32] = new VertexUi(new Vector2(_w, _h - _cy));
        VertexData[33] = new VertexUi(new Vector2(_w - _cx, _cy));
        VertexData[34] = new VertexUi(new Vector2(_w, _h - _cy));
        VertexData[35] = new VertexUi(new Vector2(_w - _cx, _h - _cy));
        // Bottom Left
        VertexData[36] = new VertexUi((_cuts & CutEdges.BottomLeft) != 0 ? new Vector2(_cx / 2f, _h - _cy / 2f) : new Vector2(0f, _h));
        VertexData[37] = new VertexUi(new Vector2(0, _h - _cy));
        VertexData[38] = new VertexUi(new Vector2(_cx, _h - _cy));
        VertexData[39] = new VertexUi((_cuts & CutEdges.BottomLeft) != 0 ? new Vector2(_cx / 2f, _h - _cy / 2f) : new Vector2(0f, _h));
        VertexData[40] = new VertexUi(new Vector2(_cx, _h - _cy));
        VertexData[41] = new VertexUi(new Vector2(_cx, _h));
        // Bottom Middle
        VertexData[42] = new VertexUi(new Vector2(_cx, _h - _cy));
        VertexData[43] = new VertexUi(new Vector2(_w - _cx, _h - _cy));
        VertexData[44] = new VertexUi(new Vector2(_w - _cx, _h));
        VertexData[45] = new VertexUi(new Vector2(_cx, _h - _cy));
        VertexData[46] = new VertexUi(new Vector2(_w - _cx, _h));
        VertexData[47] = new VertexUi(new Vector2(_cx, _h));
        // Bottom Right
        VertexData[48] = new VertexUi((_cuts & CutEdges.BottomRight) != 0 ? new Vector2(_w - _cx / 2f, _h - _cy / 2f) : new Vector2(_w, _h));
        VertexData[49] = new VertexUi(new Vector2(_w - _cx, _h));
        VertexData[50] = new VertexUi(new Vector2(_w - _cx, _h - _cy));
        VertexData[51] = new VertexUi((_cuts & CutEdges.BottomRight) != 0 ? new Vector2(_w - _cx / 2f, _h - _cy / 2f) : new Vector2(_w, _h));
        VertexData[52] = new VertexUi(new Vector2(_w - _cx, _h - _cy));
        VertexData[53] = new VertexUi(new Vector2(_w, _h - _cy));
        
        SetGraphicsBuffer();
    }

    public void Resize(int width, int height) {
        _w = width;
        _h = height;
        FillData();
    }
}