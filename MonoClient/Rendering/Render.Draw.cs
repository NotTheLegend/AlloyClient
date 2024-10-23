using Microsoft.Xna.Framework.Graphics;
using MonoClient.Assets;
using MonoClient.Rendering.VertexData;

namespace MonoClient.Rendering;

public static partial class Render {
    public static int LastDrawCountTiles;
    public static int LastDrawCountShadows;
    public static int LastDrawCountEntities;

    private static int _tileCount;
    private static int _editorTileCount;
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
        _shaderGround.CurrentTechnique.Passes[0].Apply();
        _graphics.Indices = _modelIndexBuffer;
        _graphics.SetVertexBuffers(_modelVertexBinding, _tileDataBinding);
        _graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, info.IndexOffset, info.PrimitiveCount, _tileCount);
    }

    #endregion

    #region MapEditorTile

    public static void StartDrawEditorTile() {
        LastDrawCountTiles = 0;
        _editorTileCount = 0;

        _shaderGround.CurrentTechnique.Passes[0].Apply();
        _graphics.Indices = _modelIndexBuffer;
        _graphics.SetVertexBuffers(_modelVertexBinding, _editorTileDataBinding);
    }

    public static void DrawEditorTile(VertexTile data) {
        _editorTileData[_editorTileCount] = data;
        _editorTileCount++;

        if (_editorTileCount == BufferSize) {
            FlushBufferEditorTile();
        }
    }

    public static void FlushBufferEditorTile() {
        if (_editorTileCount < 1) {
            return;
        }

        LastDrawCountTiles += _editorTileCount;

        _editorTileDataBuffer.SetData(_editorTileData, 0, _editorTileCount);

        var info = ModelData.ModelRenderInfo[ModelType.PbTile];
        _graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, info.IndexOffset, info.PrimitiveCount, _editorTileCount);
        _editorTileCount = 0;
    }

    #endregion

    #region Render Shadow

    public static void StartDrawShadow() {
        LastDrawCountShadows = 0;
        _shadowCount = 0;

        _shaderShadow.CurrentTechnique.Passes[0].Apply();
        _graphics.Indices = _modelIndexBuffer;
        _graphics.SetVertexBuffers(_modelVertexBinding, _shadowDataBinding);
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
        _graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, info.IndexOffset, info.PrimitiveCount, _shadowCount);

        LastDrawCountShadows += _shadowCount;
        _shadowCount = 0;
    }

    #endregion

    #region Render Entity

    public static void StartDrawEntity() {
        LastDrawCountEntities = 0;
        _entityCount = 0;
        _shaderObject.CurrentTechnique.Passes[0].Apply();
        _graphics.Indices = _modelIndexBuffer;
        _graphics.SetVertexBuffers(_modelVertexBinding, _entityDataBinding);
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
        _graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, info.IndexOffset, info.PrimitiveCount, _entityCount);
        _entityCount = 0;
    }

    #endregion
}