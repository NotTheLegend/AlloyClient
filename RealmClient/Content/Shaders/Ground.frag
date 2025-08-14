#version 450 core

struct InstanceData {
    vec4 Position;
    vec4 UV;
    vec4 Animate;
    vec4 Mask;
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
    
    if (data.Mask.x > -1.0){
        vec2 maskCoords = map(baseUV, data.Mask.xy, data.Mask.xy + data.Mask.zw);
        float alpha = texture(GameTexture, maskCoords).a;
        if (alpha == 0)
            discard;
        color.a = alpha;
    }

    FragColor = color;
}