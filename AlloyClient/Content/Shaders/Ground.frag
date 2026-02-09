#version 460 core

#define TileBuffer

uniform sampler2D GameTexture;
uniform vec4 AlphaBlends[8];

in GROUND_OUTPUT {
    vec2 baseUV;
    vec2 coreUV;
    vec4 UV;
    vec4 Mask;
} vsInput;

out vec4 FragColor;

float map(float value, float newMin, float newMax) {
    float val = abs(mod((value + 1), 1.0)) * (newMax - newMin) + newMin;
    return clamp(val, newMin, newMax);
}

vec2 map(vec2 values, vec2 newMins, vec2 newMaxs) {
    return vec2(map(values.x, newMins.x, newMaxs.x), map(values.y, newMins.y, newMaxs.y));
}

void main() {
    vec2 uv = map(vsInput.coreUV, vsInput.UV.xy, vsInput.UV.xy + vsInput.UV.zw);
    vec2 dx = dFdx(uv);
    vec2 dy = dFdy(uv);
    vec4 ogColor = textureGrad(GameTexture, uv, dx, dy);
    
    if (vsInput.Mask.x > -1.0){
        vec2 maskCoords = map(vsInput.baseUV, vsInput.Mask.xy, vsInput.Mask.xy + vsInput.Mask.zw);
        vec2 mdx = dFdx(maskCoords);
        vec2 mdy = dFdy(maskCoords);
        float alpha = textureGrad(GameTexture, maskCoords, mdx, mdy).a;
        ogColor.a = alpha;
    }

    FragColor = ogColor;
}