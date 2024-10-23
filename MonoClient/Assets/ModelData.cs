using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoClient.Rendering.VertexData;
using MonoClient.Utils;

namespace MonoClient.Assets;

public enum ModelType {
    Null,
    BigCube,
    BrokenPillar,
    CandyColBroken,
    CandyColWhole,
    CandyDoughnut1,
    CandyDoughnut2,
    CandyDoughnut3,
    CandyDoughnut4,
    CloningVat,
    Crate,
    Cube,
    Dodecahedron,
    GasEmitter,
    Gate,
    GateEnd1,
    GateEnd2,
    GateEntry,
    GateEntry2,
    Icosahedron,
    Jacko,
    LabTank,
    LargeMonument1,
    LargeMonument2,
    LargeMonument3,
    MonsterTank1,
    MonsterTank2,
    MonsterTank3,
    MonsterTank4,
    Monument1,
    Monument2,
    Monument3,
    Obelisk,
    Octahedron,
    Pillar,
    Pyramid,
    Sign,
    Squatty3Side,
    Table,
    TableEdge,
    Tesla,
    Tetrahedron,
    Tower,
    Web,
    PbTile,
    PbObject,
    PbWall,
    PbDoubleWall,
    PbDoubleWall2,
    PbTripleWall
}

public static partial class ModelData {
    private static readonly Logger Log = new(typeof(ModelData));
    
    public static readonly Dictionary<ModelType, ModelInfo> ModelRenderInfo = [];

    public static VertexBase[] Vertices;
    public static short[] Indices;

    private static readonly List<VertexBase> TempVertices = [];
    private static readonly List<short> TempIndices = [];

    public static void Load() {
        LoadPrebuilt();
        
        var models = Directory.GetFiles("Content/Objects", "*.xnb");

        foreach (var file in models) {
            var name = Path.GetFileNameWithoutExtension(file);
            var model = Main.ContentManager.Load<Model>("Objects/" + name);
            var mesh = model.Meshes[0].MeshParts[0];
            var vertexData = new VertexData[mesh.VertexBuffer.VertexCount];
            var indexData = new short[mesh.IndexBuffer.IndexCount];
            mesh.VertexBuffer.GetData(0, vertexData, 0, vertexData.Length, mesh.VertexBuffer.VertexDeclaration.VertexStride);
            mesh.IndexBuffer.GetData(0, indexData, 0, indexData.Length);
            Main.ContentManager.UnloadAsset("Objects/" + name);
            
            ModelType type;
            try {
                type = (ModelType) Enum.Parse(typeof(ModelType), name);
            }
            catch {
                Log.Error($"Failed to parse model type: {name}");
                continue;
            }
            
            ParseModel(new MeshData(vertexData, indexData, type), true);
        }

        Vertices = TempVertices.ToArray();
        Indices = TempIndices.ToArray();
    }

    private static void ParseModel(MeshData mesh, bool fbx = false) {
        var count = mesh.IndexData.Length / 3;
        var indexOffset = TempIndices.Count;
        var vertexOffset = TempVertices.Count;
        
        foreach (var index in mesh.IndexData) {
            TempIndices.Add((short)(vertexOffset + index));
        }
        
        foreach (var vertex in mesh.VertexData) {
            TempVertices.Add(vertex.ToBaseVertex(fbx));
        }
        
        ModelRenderInfo[mesh.ModelType] = new ModelInfo(indexOffset, count);
    }
    
    private struct MeshData(VertexData[] vertexData, short[] indexData, ModelType modelType) {
        public VertexData[] VertexData = vertexData;
        public short[] IndexData = indexData;
        public ModelType ModelType = modelType;
    }
    
    private readonly struct VertexData(Vector3 position, Vector2 uv) {
        public readonly Vector3 Position = position;
        public readonly Vector3 _;// Needed
        public readonly Vector2 UV = uv;
    }

    private static VertexBase ToBaseVertex(this VertexData vertex, bool fbx) {
        if (fbx) {
            return new VertexBase(vertex.Position, new Vector2(vertex.UV.X, 1 - vertex.UV.Y));
        }
        
        return new VertexBase(vertex.Position, vertex.UV);
    }
}

public readonly struct ModelInfo(int index, int count) {
    public readonly int IndexOffset = index;
    public readonly int PrimitiveCount = count;
}