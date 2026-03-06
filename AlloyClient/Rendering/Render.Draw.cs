using System;
using System.Diagnostics;
using AlloyClient.Assets;
using AlloyClient.Rendering.VertexData;
using Common;
using OpenTK.Graphics.OpenGL;

namespace AlloyClient.Rendering;

public static partial class Render {
    public static int LastDrawCountTiles;
    public static int LastDrawCountShadows;
    public static int LastDrawCountEntities;

    private static int _tileCount;
    private static int _shadowCount;
    private static int _modelCount;
    private static int _entityCount;
    private static int _outlineGlowCount;
    private static ModelType _entityModel;

    #region Render Tile

    public static void StartNewDrawTile() {
        LastDrawCountTiles = 0;
        _tileCount = 0;
    }

    public static void DrawNewTile(TileData data) {
        _tileData[_tileCount] = data;
        _tileCount++;
    }

    public static void EndNewDrawTile() {
        _tileBuffer.SetData(_tileData.AsSpan(0, _tileCount));
    }

    public static void DrawTiles() {
        LastDrawCountTiles = _tileCount;
        
        _defaultVao.Bind();
        
        _shaderGround.Apply();
        _tileBuffer.BindToIndex(0);
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, _tileCount * 6);
    }

    #endregion

    #region Render Shadow

    public static void StartDrawShadow() {
        LastDrawCountShadows = _shadowCount = 0;

        _defaultVao.Bind();
        _shaderShadow.SetValue("ShadowData", _shadowBuffer);
        _shaderShadow.Apply();
    }

    public static void DrawShadow(ShadowData shadow) {
        _shadowData[_shadowCount] = shadow;
        _shadowCount++;

        if (_shadowCount == _shadowData.Length) {
            FlushBufferShadow();
        }
    }

    private static void FlushBufferShadow() {
        _shadowBuffer.SetData(_shadowData.AsSpan(0, _shadowCount), 0);
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, _shadowCount * 6);
        
        LastDrawCountShadows += _shadowCount;
        _shadowCount = 0;
    }

    public static void EndShadowDraw() {
        if (_shadowCount == 0) {
            return;
        }
        
        FlushBufferShadow();
    }

    #endregion

    #region Render Model

    public static void StartDrawModel() {
        LastDrawCountEntities = 0;
        _modelCount = 0;
        
        _shaderModel.Apply();
        _modelVao.Bind();
    }

    public static void SetEntityModel(ModelType model) => _entityModel = model;

    public static void DrawModel(VertexModel vertexModel) {
        _modelData[_modelCount] = vertexModel;
        _modelCount++;

        if (_modelCount == _modelData.Length) {
            FlushBufferModel();
        }
    }

    public static void FlushBufferModel() {
        if (_modelCount < 1) {
            return;
        }
        
        _modelDataBuffer.SetData(_modelData.AsSpan(0, _modelCount));

        var info = ModelData.ModelRenderInfo[_entityModel];
        GL.DrawElementsInstanced(PrimitiveType.Triangles, info.PrimitiveCount * 3, DrawElementsType.UnsignedShort, info.IndexOffset * 2, _modelCount);
        _modelCount = 0;
    }

    #endregion
    
    
    #region Render Entity

    public static void StartDrawEntity() {
        LastDrawCountEntities = 0;
        _entityCount = 0;
        _outlineGlowCount = 0;
        
        _shaderObject.Apply();
        _entityDataBuffer.BindToIndex(0);
    }

    public static void DrawEntity(VertexObject vertexObject, bool outlineGlow = false) {
        _entityData[_entityCount] = vertexObject;
        _entityCount++;

        if (outlineGlow) {
            _outlineGlowData[_outlineGlowCount] = vertexObject;
            _outlineGlowCount++;
        }

        if (_entityCount == _entityData.Length) {
            FlushBufferEntity();
        }
    }

    public static void FlushBufferEntity() {
        if (_entityCount < 1) return;

        // Pass 1: opaque pixels only — depth writes ON, no blend
        _entityDataBuffer.SetData(_entityData.AsSpan(0, _entityCount));
        _entityDataBuffer.BindToIndex(0);
        
        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Less);
        GL.Disable(EnableCap.Blend);
        _shaderObject.SetValue("RenderPass", 0);
        GL.DrawArrays(PrimitiveType.Triangles, 0, _entityCount * 6);

        // Pass 2: glow/outline pixels only — depth writes OFF, test still rejects hidden glows
        _outlineGlowDataBuffer.SetData(_outlineGlowData.AsSpan(0, _outlineGlowCount));
        _outlineGlowDataBuffer.BindToIndex(0);
        
        GL.DepthMask(false);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.Enable(EnableCap.Blend);
        _shaderObject.SetValue("RenderPass", 1);
        GL.DrawArrays(PrimitiveType.Triangles, 0, _outlineGlowCount * 6);

        // Restore
        GL.DepthMask(true);

        _entityCount = 0;
        _outlineGlowCount = 0;
    }

    #endregion
}