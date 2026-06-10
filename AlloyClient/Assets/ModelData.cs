using System;
using System.Collections.Generic;
using System.IO;
using AlloyClient.Rendering.VertexData;
using Alloy.Common;
using AlloyClient.Logging;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;

namespace AlloyClient.Assets;

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
    PbTripleWall,
    
    Count
}

public static partial class ModelData {
    private static readonly ILogger Logger = ILogger.CreateLogger(nameof(ModelData));
    
    public static readonly Dictionary<ModelType, ModelInfo> ModelRenderInfo = [];

    public static VertexBase[] Vertices;
    public static ushort[] Indices;

    private static readonly List<VertexBase> TempVertices = [];
    private static readonly List<ushort> TempIndices = [];

    public static void Load() {
        LoadPrebuilt();
        
        var models = Directory.GetFiles("Content/Objects", "*.bin");

        foreach (var file in models) {
            var name = Path.GetFileNameWithoutExtension(file);
            
            ModelType type;
            try {
                type = (ModelType) Enum.Parse(typeof(ModelType), name);
            } catch {
                Logger.Log(LogLevel.Error, $"Failed to parse model type: {name}");
                continue;
            }

            var mesh = MeshData.Read(File.ReadAllBytes(file), type);
            ParseModel(mesh, true);
        }

        Vertices = TempVertices.ToArray();
        Indices = TempIndices.ToArray();
    }

    private static void ParseModel(MeshData mesh, bool fbx = false) {
        var count = mesh.IndexBuffer.Length / 3;
        var indexOffset = TempIndices.Count;
        var vertexOffset = TempVertices.Count;
        
        foreach (var index in mesh.IndexBuffer) {
            TempIndices.Add((ushort)(vertexOffset + index));
        }
        
        foreach (var vertex in mesh.VertexBuffer) {
            TempVertices.Add(vertex.ToBaseVertex(fbx));
        }
        
        ModelRenderInfo[mesh.ModelType] = new ModelInfo(indexOffset, count);
    }
    
    private struct MeshData(VertexData[] vertexBuffer, ushort[] indexBuffer, ModelType modelType, bool hasUV) {
        public VertexData[] VertexBuffer = vertexBuffer;
        public ushort[] IndexBuffer = indexBuffer;
        public ModelType ModelType = modelType;
        public readonly bool HasUV = hasUV;

        public static MeshData Read(byte[] fileData, ModelType modelType) {
            using var reader = new BinaryReader(new MemoryStream(fileData));

            var len = reader.ReadInt32();
            var indices = new ushort[len];

            for (var i = 0; i < len; i++) {
                indices[i] = (ushort)reader.ReadInt32();
            }

            len = reader.ReadInt32();
            var vertices = new VertexData[len];

            for (var i = 0; i < len; i++) {
                vertices[i] = VertexData.Read(reader);
            }

            var hasUV = reader.ReadBoolean();

            return new MeshData(vertices, indices, modelType, hasUV);
        }
    }
    
    private readonly struct VertexData(Vector3 position, Vector3 normal, Vector2 uv) {
        public readonly Vector3 Position = position;
        public readonly Vector3 Normal = normal;
        public readonly Vector2 UV = uv;

        public VertexData(Vector3 position, Vector2 uv) : this(position, Vector3.Zero, uv) { }

        public static VertexData Read(BinaryReader reader) => new(reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector2());
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