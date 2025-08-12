#version 450 core

uniform mat4x4 WorldMatrix;
uniform mat4x4 ViewMatrix;
uniform mat4x4 ProjMatrix;
uniform mat4x4 BillMatrix;

const vec2 shadowPos[6] = vec2[6](
    vec2(-0.5, 0.5),
    vec2(0.5, 0.5),
    vec2(-0.5, -0.5),
    vec2(-0.5, -0.5),
    vec2(0.5, 0.5),
    vec2(0.5, -0.5)
);

const vec2 shadowUV[6] = vec2[6](
    vec2(0.0, 1.0),
    vec2(1.0, 1.0),
    vec2(0.0, 0.0),
    vec2(0.0, 0.0),
    vec2(1.0, 1.0),
    vec2(1.0, 0.0)
);

struct InstanceData {
    vec4 Position;
    vec2 Scale;
    uint Color;
    float Padding;
};

layout(std140, binding = 0) readonly buffer InstanceBuffer {
    InstanceData data[];
} instanceBuffer;

out vec2 BaseUV;
out flat uint Color;

void main() {
    int instanceId = gl_VertexID / 6;
    int verId = gl_VertexID % 6;
    
    InstanceData data = instanceBuffer.data[instanceId];
    
    vec4 pos = vec4(shadowPos[verId] * data.Scale, 0, 1) * BillMatrix;
    pos.xyz += data.Position.xyz;
    
    gl_Position = pos * WorldMatrix * ViewMatrix * ProjMatrix;
    
    BaseUV = shadowUV[verId];
    Color = data.Color;
}