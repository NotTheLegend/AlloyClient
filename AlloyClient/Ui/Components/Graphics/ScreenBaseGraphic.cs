using System;
using AlloyClient.UiLib.Core;
using AlloyClient.UiLib.Enums;
using AlloyClient.UiLib.Rendering;
using OpenTK.Mathematics;
using AlloyClient.Utils;

namespace AlloyClient.Ui.Components.Graphics;

public sealed class ScreenGraphic : Sprite {
    
    private const int TexWidth = 1280;
    private const int TexHeight = 720;

    public ScreenGraphic(bool splash = false) {

        TextureId = splash ? TextureType.TitleGraphic : TextureType.TitleBackground;
        
        ResizeBackBuffer();
        FillData(TexWidth, TexHeight);
        
        AddEventListener(Event.AddedToStage, OnStageEnter);
        AddEventListener(Event.RemovedFromStage, OnStageExit);
    }

    private void OnStageEnter() {
        Stage.AddEventListener(ResizeEvent.Resize, OnResize);
        OnResize(new ResizeEvent("", Stage.StageWidth, Stage.StageHeight));
    }

    private void OnStageExit() {
        Stage.RemoveEventListener(ResizeEvent.Resize, OnResize);
    }

    private void OnResize(ResizeEvent args) {
        var x = (float)args.Width / TexWidth;
        var y = (float)args.Height / TexHeight;

        var scale = MathF.Max(x, y);

        var w = (int)(TexWidth * scale);
        var h = (int)(TexHeight * scale);

        X = (w - Stage.StageWidth) / -2;
        Y = (h - Stage.StageHeight) / -2;
        
        FillData(w, h);
    }
    
    private void ResizeBackBuffer() {
        VertexData = new VertexUi[4];
        Indices = [0, 1, 2, 0, 2, 3];
    }

   private void FillData(int width, int height) {
       VertexData[0] = new VertexUi(new Vector2(0, height), new Vector2(0f, 1f));
       VertexData[1] = new VertexUi(new Vector2(0, 0), new Vector2(0f, 0f));
       VertexData[2] = new VertexUi(new Vector2(width, 0), new Vector2(1f, 0f));
       VertexData[3] = new VertexUi(new Vector2(width, height), new Vector2(1f, 1f));
       
       SetGraphicsBuffer();
    }
}