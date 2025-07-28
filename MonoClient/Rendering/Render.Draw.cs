using System;
using MonoClient.Assets;
using MonoClient.Rendering.VertexData;
using OpenTK.Graphics.OpenGL;

namespace MonoClient.Rendering;

public static partial class Render {
    public static int LastDrawCountTiles;
    public static int LastDrawCountShadows;
    public static int LastDrawCountEntities;

    private static int _tileCount;
    private static int _shadowCount;
    private static int _entityCount;
    private static ModelType _entityModel;

    #region Render Tile

    public static void StartDrawTile() {
        LastDrawCountTiles = 0;
        _tileCount = 0;
    }

    public static void DrawTile(VertexTile data) {
        _tileData[_tileCount] = data;
        _tileCount++;
    }

    public static void EndDrawTile() {
        _tileDataBuffer.SetData(_tileData);
    }

    public static void FlushBufferTile() {
        LastDrawCountTiles = _tileCount;

        var info = ModelData.ModelRenderInfo[ModelType.PbTile];
        _shaderGround.Apply();
        GL.BindVertexArray(_tileVao);
        //TODO: i think this is right, but i could be way off
        GL.DrawElementsInstanced(PrimitiveType.Triangles, info.PrimitiveCount * 3, DrawElementsType.UnsignedShort, info.IndexOffset * 2, _tileCount);
    }

    #endregion

    #region Render Shadow

    public static void StartDrawShadow() {
        LastDrawCountShadows = 0;
        _shadowCount = 0;

        _shaderShadow.Apply();
        GL.BindVertexArray(_shadowVao);
    }

    public static void DrawShadow(VertexShadow shadow) {
        _shadowData[_shadowCount] = shadow;
        _shadowCount++;

        if (_shadowCount == BufferSize) {
            FlushBufferShadow();
        }
    }

    public static void FlushBufferShadow() {
        if (_shadowCount == 0) {
            return;
        }

        _shadowDataBuffer.SetData(_shadowData, 0, _shadowCount);

        var info = ModelData.ModelRenderInfo[ModelType.PbObject];
        GL.DrawElementsInstanced(PrimitiveType.Triangles, info.PrimitiveCount * 3, DrawElementsType.UnsignedShort, info.IndexOffset * 2, _shadowCount);

        LastDrawCountShadows += _shadowCount;
        _shadowCount = 0;
    }

    #endregion

    #region Render Entity

    public static void StartDrawEntity() {
        LastDrawCountEntities = 0;
        _entityCount = 0;
        
        _shaderObject.Apply();
        GL.BindVertexArray(_entityVao);
    }

    public static void SetEntityModel(ModelType model) => _entityModel = model;

    public static void DrawEntity(VertexObject vertexObject) {
        _entityData[_entityCount] = vertexObject;
        _entityCount++;

        if (_entityCount == _entityData.Length) {
            FlushBufferEntity();
        }
    }

    public static void FlushBufferEntity() {
        if (_entityCount < 1) {
            return;
        }
        
        _entityDataBuffer.SetData(_entityData, 0, _entityCount);

        var info = ModelData.ModelRenderInfo[_entityModel];
        GL.DrawElementsInstanced(PrimitiveType.Triangles, info.PrimitiveCount * 3, DrawElementsType.UnsignedShort, info.IndexOffset * 2, _entityCount);
        _entityCount = 0;
    }

    #endregion
}