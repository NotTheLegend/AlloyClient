using AlloyClient.Assets;
using AlloyClient.Rendering.VertexData;
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
        _tileBuffer.SetData(_tileData, 0, _tileCount);
    }

    public static void DrawTiles() {
        LastDrawCountTiles = _tileCount;
        
        GL.BindVertexArray(_defaultVao);
        
        _shaderGround.Apply();
        _tileBuffer.BindToIndex(0);
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, _tileCount * 6);
    }

    #endregion

    #region Render Shadow

    public static void StartDrawShadow() {
        LastDrawCountShadows = 0;
        _shadowCount = 0;

        GL.BindVertexArray(_defaultVao);
        _shadowBuffer.BindToIndex(0);
        _shaderShadow.Apply();
    }

    public static void DrawShadow(ShadowData shadow) {
        _shadowData[_shadowCount] = shadow;
        _shadowCount++;

        if (_shadowCount == BufferSize) {
            FlushBufferShadow();
        }
    }

    private static void FlushBufferShadow() {
        _shadowBuffer.SetData(_shadowData, 0, _shadowCount);
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, _shadowCount * 6);
        
        LastDrawCountShadows += _shadowCount;
        _shadowCount = 0;
    }

    public static void EndShadowDraw() {
        if (_shadowCount == 0) return;

        _shadowBuffer.SetData(_shadowData, 0, _shadowCount);
        
        GL.DrawArrays(PrimitiveType.Triangles, 0, _shadowCount * 6);

        LastDrawCountShadows += _shadowCount;
        _shadowCount = 0;
    }

    #endregion

    #region Render Model

    public static void StartDrawModel() {
        LastDrawCountEntities = 0;
        _modelCount = 0;
        
        _shaderModel.Apply();
        GL.BindVertexArray(_modelVao);
    }

    //public static void SetEntityModel(ModelType model) => _entityModel = model;

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
        
        _modelDataBuffer.SetData(_modelData, 0, _modelCount);

        var info = ModelData.ModelRenderInfo[_entityModel];
        GL.DrawElementsInstanced(PrimitiveType.Triangles, info.PrimitiveCount * 3, DrawElementsType.UnsignedShort, info.IndexOffset * 2, _modelCount);
        _modelCount = 0;
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