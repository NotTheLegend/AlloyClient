#version 460 core

#define TileBuffer 

uniform mat4 WorldMatrix;
uniform mat4 ViewMatrix;
uniform mat4 ProjMatrix;
uniform float GameTime;

const vec2 tilePos[6] = vec2[6](
    vec2(0, 1),
    vec2(1, 1),
    vec2(0, 0),
    vec2(0, 0),
    vec2(1, 1),
    vec2(1, 0)
);

const vec2 tileUV[6] = vec2[6](
    vec2(0.0, 1.0),
    vec2(1.0, 1.0),
    vec2(0.0, 0.0),
    vec2(0.0, 0.0),
    vec2(1.0, 1.0),
    vec2(1.0, 0.0)
);

struct InstanceData {
    vec4 Position;
    vec4 UV;
    vec4 Animate;
    vec4 Mask;
};

layout(std140, binding = 0) readonly buffer InstanceBuffer {
    InstanceData data[TileBuffer];
} instanceBuffer;

out GROUND_OUTPUT {
    vec2 baseUV;
    vec2 coreUV;
    vec4 UV;
    vec4 Mask;
} vsOutput;

void main() {
    int instanceId = gl_VertexID / 6;
    int verId = gl_VertexID % 6;
    
    InstanceData data = instanceBuffer.data[instanceId];
    
    vec4 inputPosition = vec4(tilePos[verId], 0, 1);
    inputPosition.xy += data.Position.xy;
    gl_Position = inputPosition * WorldMatrix * ViewMatrix * ProjMatrix;

    vsOutput.baseUV = tileUV[verId];
    vsOutput.coreUV.x = tileUV[verId].x + data.Position.z + sin(GameTime * data.Animate.x) + GameTime * data.Animate.z;
    vsOutput.coreUV.y = tileUV[verId].y + data.Position.w + sin(GameTime * data.Animate.y) + GameTime * data.Animate.w;
    vsOutput.UV = data.UV;
    vsOutput.Mask = data.Mask;
}