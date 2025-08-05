#version 450 core

uniform mat4x4 WorldMatrix;
uniform mat4x4 ViewMatrix;
uniform mat4x4 ProjMatrix;
uniform mat4x4 BillMatrix;

const vec2 particlePos[6] = vec2[6](
    vec2(-0.1, 0.1),
    vec2(0.1, 0.1),
    vec2(-0.1, -0.1),
    vec2(-0.1, -0.1),
    vec2(0.1, 0.1),
    vec2(0.1, -0.1)
);

const vec2 particleUV[6] = vec2[6](
    vec2(0.0, 1.0),
    vec2(1.0, 1.0),
    vec2(0.0, 0.0),
    vec2(0.0, 0.0),
    vec2(1.0, 1.0),
    vec2(1.0, 0.0)
);

struct InstanceData {
    vec4 Position;
    vec4 Color;
};

layout(std140, binding = 0) readonly buffer InstanceBuffer {
    InstanceData data[];
} instanceBuffer;

out vec2 BaseUV;
out vec4 Color;
out float Depth;

void main() {
    int instanceId = gl_VertexID / 6;
    int verId = gl_VertexID % 6;
    
    InstanceData data = instanceBuffer.data[instanceId];
    
    vec4 pos = vec4(particlePos[verId], 0, 1.0) * BillMatrix;
    pos.xyz += data.Position.xyz;

    gl_Position = pos * WorldMatrix * ViewMatrix * ProjMatrix;
    BaseUV = particleUV[verId];
    Color = data.Color;
    
    vec4 depth = vec4(data.Position.xy, 0, 1) * WorldMatrix * ViewMatrix * ProjMatrix;
    Depth = 0.5f + 0.4f * depth.y;
}