#version 460 core

#define ShadowBuffer 

uniform mat4 FullMatrix;
uniform mat4 BillMatrix;

const vec2 shadowPos[6] = vec2[6](
    vec2(-0.5, 0.25),
    vec2(0.5, 0.25),
    vec2(-0.5, -0.25),
    vec2(-0.5, -0.25),
    vec2(0.5, 0.25),
    vec2(0.5, -0.25)
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
    vec2 Position;
    float Scale;
    uint Color;
};

layout(std140) uniform ShadowData {
    InstanceData data[ShadowBuffer];
} instanceBuffer;

out vec2 BaseUV;
out flat uint Color;

void main() {
    int instanceId = gl_VertexID / 6;
    int verId = gl_VertexID % 6;
    
    InstanceData data = instanceBuffer.data[instanceId];
    
    vec4 pos = vec4(shadowPos[verId] * data.Scale, 0, 1) * BillMatrix;
    pos.xy += data.Position.xy;
    
    gl_Position = pos * FullMatrix;
    
    BaseUV = shadowUV[verId];
    Color = data.Color;
}