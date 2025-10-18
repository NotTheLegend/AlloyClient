using OpenTK.Mathematics;
using RealmClient.UiLib.Core;
using RealmClient.UiLib.Enums;
using RealmClient.UiLib.Rendering;

namespace RealmClient.UiLib.BuiltIn;

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
        SetAnchor(config.Anchor);
        MouseEnabled = config.MouseEnabled;
        
        TextureId = TextureType.Color;
        
        SetHitboxType(CollisionType.Vertices);

        ResizeBackBuffer();
        FillData();
    }

    private void ResizeBackBuffer() {
        VertexData = new VertexUi[36];
        Indices = new ushort[] {
            0, 1, 2, 0, 2, 3,
            4, 5, 6, 4, 6, 7,
            8, 9, 10, 8, 10, 11,
            
            12, 13, 14, 12, 14, 15,
            16, 17, 18, 16, 18, 19,
            20, 21, 22, 20, 22, 23,
            
            24, 25, 26, 24, 26, 27,
            28, 29, 30, 28, 30, 31,
            32, 33, 34, 32, 34, 35
        };
    }

    private void FillData() {
        // Top Left
        VertexData[0] = new VertexUi((_cuts & CutEdges.TopLeft) != 0 ? new Vector2(_cx / 2f, _cy / 2f) : new Vector2(0f, 0f));
        VertexData[1] = new VertexUi(new Vector2(_cx, 0f));
        VertexData[2] = new VertexUi(new Vector2(_cx, _cy));
        VertexData[3] = new VertexUi(new Vector2(0f, _cy));
        // Top Center
        VertexData[4] = new VertexUi(new Vector2(_cx, 0));
        VertexData[5] = new VertexUi(new Vector2(_w - _cx, 0));
        VertexData[6] = new VertexUi(new Vector2(_w - _cx, _cy));
        VertexData[7] = new VertexUi(new Vector2(_cx, _cy));
        // Top Right
        VertexData[8] = new VertexUi((_cuts & CutEdges.TopRight) != 0 ? new Vector2(_w - _cx / 2f, _cy / 2f) : new Vector2(_w, 0f));
        VertexData[9] = new VertexUi(new Vector2(_w, _cy));
        VertexData[10] = new VertexUi(new Vector2(_w - _cx, _cy));
        VertexData[11] = new VertexUi(new Vector2(_w - _cx, 0f));
        // Middle Left
        VertexData[12] = new VertexUi(new Vector2(0, _cy));
        VertexData[13] = new VertexUi(new Vector2(_cx, _cy));
        VertexData[14] = new VertexUi(new Vector2(_cx, _h - _cy));
        VertexData[15] = new VertexUi(new Vector2(0, _h - _cy));
        // Middle
        VertexData[16] = new VertexUi(new Vector2(_cx, _cy));
        VertexData[17] = new VertexUi(new Vector2(_w - _cx, _cy));
        VertexData[18] = new VertexUi(new Vector2(_w - _cx, _h - _cy));
        VertexData[19] = new VertexUi(new Vector2(_cx, _h - _cy));
        // Middle Right
        VertexData[20] = new VertexUi(new Vector2(_w - _cx, _cy));
        VertexData[21] = new VertexUi(new Vector2(_w, _cy));
        VertexData[22] = new VertexUi(new Vector2(_w, _h - _cy));
        VertexData[23] = new VertexUi(new Vector2(_w - _cx, _h - _cy));
        // Bottom Left
        VertexData[24] = new VertexUi((_cuts & CutEdges.BottomLeft) != 0 ? new Vector2(_cx / 2f, _h - _cy / 2f) : new Vector2(0f, _h));
        VertexData[25] = new VertexUi(new Vector2(0, _h - _cy));
        VertexData[26] = new VertexUi(new Vector2(_cx, _h - _cy));
        VertexData[27] = new VertexUi(new Vector2(_cx, _h));
        // Bottom Middle
        VertexData[28] = new VertexUi(new Vector2(_cx, _h - _cy));
        VertexData[29] = new VertexUi(new Vector2(_w - _cx, _h - _cy));
        VertexData[30] = new VertexUi(new Vector2(_w - _cx, _h));
        VertexData[31] = new VertexUi(new Vector2(_cx, _h));
        // Bottom Right
        VertexData[32] = new VertexUi((_cuts & CutEdges.BottomRight) != 0 ? new Vector2(_w - _cx / 2f, _h - _cy / 2f) : new Vector2(_w, _h));
        VertexData[33] = new VertexUi(new Vector2(_w - _cx, _h));
        VertexData[34] = new VertexUi(new Vector2(_w - _cx, _h - _cy));
        VertexData[35] = new VertexUi(new Vector2(_w, _h - _cy));
        
        SetGraphicsBuffer();
    }

    public void Resize(int width, int height) {
        _w = width;
        _h = height;
        FillData();
    }
}