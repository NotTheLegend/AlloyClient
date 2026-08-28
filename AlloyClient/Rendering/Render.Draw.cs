using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Alloy.Engine.Diagnostics;
using AlloyClient.Assets;
using AlloyClient.Rendering.VertexData;
using OpenTK.Graphics.OpenGL;

namespace AlloyClient.Rendering;

public static partial class Render {
    public static int LastDrawCountTiles;
    public static int LastDrawCountShadows;
    public static int LastDrawCountEntities;
    
    private static int _shadowCount;
    private static int _modelCount;
    private static ModelType _entityModel;

    #region Render Tile

    public static void DrawTiles(ReadOnlySpan<TileData> span) {
        UploadTiles(span);
        DrawUploadedTiles(span.Length);
    }

    public static void UploadTiles(ReadOnlySpan<TileData> span) {
        _tileBuffer.SetData(span);
        TileUploadVersion++;
    }

    public static void DrawUploadedTiles(int count) {
        if (count < 1) {
            return;
        }

        LastDrawCountTiles = count;
        _defaultVao.Bind();
        _shaderGround.Apply();
        _tileBuffer.BindToIndex(0);

        GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, count);
        FrameMetrics.RecordDrawCall();
    }

    #endregion

    #region Render Shadow

    public static void StartDrawShadow() {
        _shadowCount = 0;

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
        FrameMetrics.RecordDrawCall();
        
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
        _modelCount = 0;
        
        _shaderModel.Apply();
        _modelVao.Bind();
    }

    public static void SetEntityModel(ModelType model) => _entityModel = model;

    public static void DrawModel(VertexModel vertexModel) {
        _modelData[_modelCount] = vertexModel;
        _modelCount++;
        LastDrawCountEntities++;

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
        FrameMetrics.RecordDrawCall();
        _modelCount = 0;
    }

    #endregion
    
    
    #region Render Entity

    public static void StartDrawEntity() {
        _shaderObject.Apply();
        _entityDataBuffer.BindToIndex(0);
    }

    public static void FlushBufferEntity(List<VertexObject> targets) {
        if (targets.Count < 1) {
            return;
        }

        var chunks = (targets.Count + _entityData.Length - 1) / _entityData.Length;
        var span = CollectionsMarshal.AsSpan(targets);
        span.Sort();
        LastDrawCountEntities += targets.Count;
        
        for (var i = 0; i < chunks; i++) {
            // Pass 1: opaque pixels only — depth writes ON, no blend
            var start = i * _entityData.Length;
            var len = Math.Min(_entityData.Length, span.Length - start);
            _entityDataBuffer.SetData(span.Slice(start, len));

            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Less);
            GL.Disable(EnableCap.Blend);
            _shaderObject.SetValue("RenderPass", 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, len * 6);
            FrameMetrics.RecordDrawCall();

            // Pass 2: glow/outline pixels only — depth writes OFF, test still rejects hidden glows
            GL.DepthMask(false);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.Blend);
            _shaderObject.SetValue("RenderPass", 1);
            GL.DrawArrays(PrimitiveType.Triangles, 0, len * 6);
            FrameMetrics.RecordDrawCall();

            // Restore
            GL.DepthMask(true);
        }
    }

    #endregion
}
