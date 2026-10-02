using System;
using System.Runtime.InteropServices;
using Alloy.Common;
using Alloy.Engine.Graphics.Buffers;
using Alloy.UiLib.Extra;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Alloy.UiLib.Rendering;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SpriteInstanceData(SpriteVertexMatrix data, Color color, Color colorOverride, Vector2 info, Vector4 scissor, Vector4 extra1, Vector4 extra2, ColorTransform colorTransform) : IBufferData<SpriteInstanceData> {

    // Vertex Changes
    public Vector2 VertexScale = data.Scale;
    public Vector2 VertexRotation = new(data.Rotation, 0f);
    public Vector2 VertexOffset = data.Offset;
    public Vector2 VertexAnchor = data.Anchor;
    
    // Sprite Data
    public uint Color = color;
    public uint ColorOverride = colorOverride;
    public Vector2 Info = info;
    public Vector4 Scissor = scissor;
    public Vector4 Extra1 = extra1;
    public Vector4 Extra2 = extra2;
    public Vector4 ColorTransform = colorTransform;
    
    public static unsafe int Size { get; } = sizeof(SpriteInstanceData);

    public bool Equals(SpriteInstanceData other) {
        return Color == other.Color && 
               ColorOverride == other.ColorOverride && 
               Info.Equals(other.Info) && 
               Scissor.Equals(other.Scissor) && 
               Extra1.Equals(other.Extra1) && 
               Extra2.Equals(other.Extra2) && 
               ColorTransform.Equals(other.ColorTransform);
    }

    public override bool Equals(object obj) {
        return obj is SpriteInstanceData other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Color, ColorOverride, Info, Scissor, Extra1, Extra2, ColorTransform);
    }
}

internal readonly record struct SpriteVertexMatrix(Vector2 Scale, float Rotation, Vector2 Offset, Vector2 Anchor);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct SpriteVertexData(VertexUi vertex, uint instanceId) : IVertexData<SpriteVertexData> {
    
    public Vector2 Position = vertex.Position;
    public Vector2 UV = vertex.UV;
    public uint Color = vertex.Color;
    public uint InstanceId = instanceId;
    
    public static VertexStride VertexStride { get; } = new([
        new ElementFormat(0, VertexAttribType.Float, FormatType.Vector2),
        new ElementFormat(1, VertexAttribType.Float, FormatType.Vector2),
        new ElementFormat(2, VertexAttribType.UnsignedInt, FormatType.Default),
        new ElementFormat(3, VertexAttribType.UnsignedInt, FormatType.Default),
    ]);

    public bool Equals(SpriteVertexData other) {
        return Color == other.Color && 
               Position == other.Position && 
               UV.Equals(other.UV) && 
               InstanceId == other.InstanceId;
    }

    public override bool Equals(object obj) {
        return obj is SpriteVertexData other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Position, UV, Color, InstanceId);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexUi {
    
    public Vector2 Position;
    public Vector2 UV;
    public Color Color;
    //internal int StateIndex;

    public VertexUi(Vector2 pos, Vector2 uv, Color color) {
        Position = pos;
        UV = uv;
        Color = color;
    }
    
    public VertexUi(Vector2 pos, Vector2 uv) {
        Position = pos;
        UV = uv;
        Color = new Color(0);
    }
    
    public VertexUi(Vector2 pos, Color color) {
        Position = pos;
        UV = new Vector2(0f);
        Color = color;
    }

    public VertexUi(Vector2 pos) {
        Position = pos;
        UV = new Vector2(0f);
        Color = new Color(0);
    }
}