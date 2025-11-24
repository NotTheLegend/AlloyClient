#version 450 core

uniform mat4 ViewMatrix;

struct InstanceData {
    // Vertex Changes
    vec2 VertexScale;
    vec2 VertexRotation;
    vec2 VertexOffset;
    vec2 VertexAnchor;

    // Sprite Data
    uint Color;
    uint ColorOverride;
    vec2 Info;
    vec4 Scissor;
    vec4 Extra1;
    vec4 Extra2;
    vec4 ColorTransform;
};

layout(std140, binding = 0) readonly buffer InstanceBuffer {
    InstanceData data[];
} instanceBuffer;

layout (location = 0) in vec2 Position;
layout (location = 1) in vec2 UVCoords;
layout (location = 2) in uint Color;
layout (location = 3) in uint InstanceId;

out VS_OUT {
    vec4 Position1;
    flat uint Color;
    flat uint Override;
    vec2 Info;
    vec2 UVCoords;
    vec4 Scissor;
    vec4 Extra1;
    vec4 Extra2;
    vec4 ColorTransform;
} vsOutput;

void main() {
    InstanceData data = instanceBuffer.data[InstanceId];
    float rotation = data.VertexRotation.x;
    vec2 pos = Position + data.VertexAnchor;
    float x = (pos.x * cos(rotation) - pos.y * sin(rotation) - data.VertexAnchor.x) * data.VertexScale.x + data.VertexOffset.x;
    float y = (pos.x * sin(rotation) + pos.y * cos(rotation) - data.VertexAnchor.y) * data.VertexScale.y + data.VertexOffset.y;
    pos = vec2(x, y);
    
    gl_Position = vec4(pos, 0, 1) * ViewMatrix;
    vsOutput.Position1 = gl_Position;
    vsOutput.Color = mix(Color, data.Color, Color == 0);
    vsOutput.Override = data.ColorOverride;
    vsOutput.Info = data.Info;
    vsOutput.UVCoords = UVCoords;
    vsOutput.Scissor.xy = (vec4(data.Scissor.x, data.Scissor.y, 0, 1) * ViewMatrix).xy;
    vsOutput.Scissor.zw = (vec4(data.Scissor.z, data.Scissor.w, 0, 1) * ViewMatrix).xy;
    vsOutput.Extra1 = data.Extra1;
    vsOutput.Extra2 = data.Extra2;
    vsOutput.ColorTransform = data.ColorTransform;
}