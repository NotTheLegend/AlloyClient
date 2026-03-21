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

void main() {
    vec2 tileTexelSize = round(vsInput.UV.zw * 4096.0);
    vec2 tileTexelOrigin = round(vsInput.UV.xy * 4096.0);
    vec2 wrappedTexels = floor(fract(vsInput.coreUV) * tileTexelSize);
    vec4 ogColor = texture(GameTexture, (tileTexelOrigin + wrappedTexels + 0.5) / 4096.0);

    if (vsInput.Mask.x > -1.0) {
        vec2 maskTexelSize = round(vsInput.Mask.zw * 4096.0);
        vec2 maskTexelOrigin = round(vsInput.Mask.xy * 4096.0);
        vec2 maskTexels = floor(vsInput.baseUV * maskTexelSize);
        float alpha = texture(GameTexture, (maskTexelOrigin + maskTexels + 0.5) / 4096.0).a;
        ogColor.a = alpha;
    }

    FragColor = ogColor;
}