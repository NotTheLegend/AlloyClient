using OpenTK.Mathematics;

namespace MonoClient.Assets;

public static partial class ModelData {
    private static void LoadPrebuilt() {
        ParseModel(FlatSquare());
        ParseModel(GameObject());
        
        ParseModel(Wall());
        ParseModel(DoubleWall());
        
        // TODO: Add all models
    }

    private static MeshData FlatSquare() {
        var mesh = new MeshData {
            VertexBuffer = [
                new VertexData(new Vector3(0, 0, 0), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(1, 0, 0), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(1, 1, 0), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(0, 1, 0), new Vector2(0, 1)) // BR
            ],
            IndexBuffer = [0, 1, 2, 0, 2, 3],
            ModelType = ModelType.PbTile
        };
        return mesh;
    }
    
    private static MeshData GameObject() {
        var mesh = new MeshData {
            VertexBuffer = [
                new VertexData(new Vector3(-0.5f, -0.5f, 0), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(0.5f, -0.5f, 0), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(0.5f, 0.5f, 0), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(-0.5f, 0.5f, 0), new Vector2(0, 1)) // BR
            ],
            IndexBuffer = [0, 1, 2, 0, 2, 3],
            ModelType = ModelType.PbObject
        };
        return mesh;
    }

    private static MeshData Wall() {
        var mesh = new MeshData {
            VertexBuffer = [
                // Front
                new VertexData(new Vector3(0, 1, 1), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(1, 1, 1), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(1, 1, 0), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(0, 1, 0), new Vector2(0, 1)), // BR
                // Back
                new VertexData(new Vector3(1, 0, 1), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(0, 0, 1), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(0, 0, 0), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(1, 0, 0), new Vector2(0, 1)), // BR
                // Left
                new VertexData(new Vector3(0, 0, 1), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(0, 1, 1), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(0, 1, 0), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(0, 0, 0), new Vector2(0, 1)), // BR
                // Right
                new VertexData(new Vector3(1, 1, 1), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(1, 0, 1), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(1, 0, 0), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(1, 1, 0), new Vector2(0, 1)) // BR
            ],
            IndexBuffer = [
                0, 1, 2, 0, 2, 3, 
                4, 5, 6, 4, 6, 7,
                8, 9, 10, 8, 10, 11,
                12, 13, 14, 12, 14, 15
            ],
            ModelType = ModelType.PbWall
        };
        return mesh;
    }
    
    private static MeshData DoubleWall() {
        var mesh = new MeshData {
            VertexBuffer = [
                // L Front
                new VertexData(new Vector3(0, 1, 1), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(1, 1, 1), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(1, 1, 0), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(0, 1, 0), new Vector2(0, 1)), // BR
                // L Back
                new VertexData(new Vector3(1, 0, 1), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(0, 0, 1), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(0, 0, 0), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(1, 0, 0), new Vector2(0, 1)), // BR
                // L Left
                new VertexData(new Vector3(0, 0, 1), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(0, 1, 1), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(0, 1, 0), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(0, 0, 0), new Vector2(0, 1)), // BR
                // L Right
                new VertexData(new Vector3(1, 1, 1), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(1, 0, 1), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(1, 0, 0), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(1, 1, 0), new Vector2(0, 1)), // BR
                // Front
                new VertexData(new Vector3(0, 1, 2), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(1, 1, 2), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(1, 1, 1), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(0, 1, 1), new Vector2(0, 1)), // BR
                // Back
                new VertexData(new Vector3(1, 0, 2), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(0, 0, 2), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(0, 0, 1), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(1, 0, 1), new Vector2(0, 1)), // BR
                // Left
                new VertexData(new Vector3(0, 0, 2), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(0, 1, 2), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(0, 1, 1), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(0, 0, 1), new Vector2(0, 1)), // BR
                // Right
                new VertexData(new Vector3(1, 1, 2), new Vector2(0, 0)), // TL
                new VertexData(new Vector3(1, 0, 2), new Vector2(1, 0)), // TR
                new VertexData(new Vector3(1, 0, 1), new Vector2(1, 1)), // BL
                new VertexData(new Vector3(1, 1, 1), new Vector2(0, 1)) // BR
            ],
            IndexBuffer = [
                0, 1, 2, 0, 2, 3, 
                4, 5, 6, 4, 6, 7,
                8, 9, 10, 8, 10, 11,
                12, 13, 14, 12, 14, 15,
            
                16, 17, 18, 16, 18, 19,
                20, 21, 22, 20, 22, 23,
                24, 25, 26, 24, 26, 27,
                28, 29, 30, 28, 30, 31
            ],
            ModelType = ModelType.PbDoubleWall
        };
        return mesh;
    }
}