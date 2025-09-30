#version 450 core

#define TileBuffer

struct InstanceData {
    vec4 Position;
    vec4 UV;
    vec4 Animate;
    vec4 Mask;
};

layout(std140, binding = 0) readonly buffer InstanceBuffer {
    InstanceData data[TileBuffer];
} instanceBuffer;


flat in int instanceId;
in vec2 baseUV;
in vec2 coreUV;

out vec4 FragColor;

uniform sampler2D GameTexture;
uniform vec4 AlphaBlends[8];

float map(float value, float newMin, float newMax) {
    float val = abs(mod((value + 1), 1.0)) * (newMax - newMin) + newMin;
    return clamp(val, newMin, newMax);
}

vec2 map(vec2 values, vec2 newMins, vec2 newMaxs) {
    return vec2(map(values.x, newMins.x, newMaxs.x), map(values.y, newMins.y, newMaxs.y));
}

vec2 uv_aa_smoothstep( vec2 uv, vec2 res, float width ) {
    vec2 pixels = uv * res;

    vec2 pixels_floor = floor(pixels + 0.5);
    vec2 pixels_fract = fract(pixels + 0.5);
    vec2 pixels_aa = fwidth(pixels) * width * 0.5;
    pixels_fract = smoothstep( vec2(0.5) - pixels_aa, vec2(0.5) + pixels_aa, pixels_fract );

    return (pixels_floor + pixels_fract - 0.5) / res;
}

vec2 uv_nearest(vec2 uv) {
    const vec2 texture_size = vec2(4096, 4096);
    vec2 pixel = uv * texture_size;
    pixel = floor(pixel) + .5;

    return pixel / texture_size;
}

void main() {
    InstanceData data = instanceBuffer.data[instanceId];
    
    vec2 ogCoords = map(coreUV, data.UV.xy, data.UV.xy + data.UV.zw);
    vec4 ogColor = texture(GameTexture, uv_aa_smoothstep(ogCoords, vec2(4096, 4096), 1.5));
    ogColor /= ogColor.a;
    
    if (data.Mask.x > -1.0){
        vec2 maskCoords = map(baseUV, data.Mask.xy, data.Mask.xy + data.Mask.zw);
        float alpha = texture(GameTexture, uv_nearest(maskCoords)).a;
        if (alpha == 0)
            discard;
        ogColor.a = alpha;
    }

    FragColor = ogColor;
}