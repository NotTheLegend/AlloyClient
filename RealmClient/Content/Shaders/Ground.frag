#version 450 core

struct InstanceData {
    vec4 Position;
    vec4 UV;
    vec4 Animate;
    vec4 BlendLeftRight;
    vec4 BlendTopBottom;
    vec4 CornerBottom;
    vec4 CornerTop;
};

layout(std140, binding = 0) readonly buffer InstanceBuffer {
    InstanceData data[];
} instanceBuffer;


flat in int instanceId;
in vec2 baseUV;
in vec2 coreUV;

out vec4 FragColor;

uniform sampler2D GameTexture;
uniform vec4 AlphaBlends[8];

const float atlasPad = 1.0 / 16.0 / 4096;

float map(float value, float newMin, float newMax) {
    float val = abs(mod((value + 1), 1.0)) * (newMax - newMin) + newMin;
    return clamp(val, newMin + atlasPad, newMax - atlasPad);
}

vec2 map(vec2 values, vec2 newMins, vec2 newMaxs) {
    return vec2(map(values.x, newMins.x, newMaxs.x), map(values.y, newMins.y, newMaxs.y));
}

void main() {
    InstanceData data = instanceBuffer.data[instanceId];
    
    vec2 ogCoords = map(coreUV, data.UV.xy, data.UV.xy + data.UV.zw);
    vec4 ogColor = texture(GameTexture, ogCoords);
    vec4 color = ogColor;

    vec2 uv = data.UV.zw;
    float highAlpha = 0;
    vec4 uva = vec4(0);
    float alpha = 0;
    vec2 uvb = vec2(0);

    uvb = data.BlendLeftRight.xy;
    uva = AlphaBlends[0];
    alpha = texture(GameTexture, map(baseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = mix(ogColor, texture(GameTexture, map(coreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }

    uvb = data.BlendLeftRight.zw;
    uva = AlphaBlends[1];
    alpha = texture(GameTexture, map(baseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = mix(ogColor, texture(GameTexture, map(coreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }

    uvb = data.BlendTopBottom.zw;
    uva = AlphaBlends[2];
    alpha = texture(GameTexture, map(baseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = mix(ogColor, texture(GameTexture, map(coreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }

    uvb = data.BlendTopBottom.xy;
    uva = AlphaBlends[3];
    alpha = texture(GameTexture, map(baseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = mix(ogColor, texture(GameTexture, map(coreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }

    uvb = data.CornerBottom.xy;
    uva = AlphaBlends[4];
    alpha = texture(GameTexture, map(baseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = mix(ogColor, texture(GameTexture, map(coreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }

    uvb = data.CornerBottom.zw;
    uva = AlphaBlends[5];
    alpha = texture(GameTexture, map(baseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = mix(ogColor, texture(GameTexture, map(coreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }

    uvb = data.CornerTop.xy;
    uva = AlphaBlends[6];
    alpha = texture(GameTexture, map(baseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = mix(ogColor, texture(GameTexture, map(coreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }

    uvb = data.CornerTop.zw;
    uva = AlphaBlends[7];
    alpha = texture(GameTexture, map(baseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = mix(ogColor, texture(GameTexture, map(coreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }

    FragColor = color;
}