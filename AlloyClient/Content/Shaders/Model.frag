#version 460 core

uniform sampler2D GameTexture;

in MODEL_OUT {
    vec2 BaseUV;
    vec4 UV;
    vec3 Extra;
    float Zed;
} input1;

out vec4 FragColor;

vec2 map(vec2 base, vec2 uvMin, vec2 uvMax) {
    return vec2(base.x * (uvMax.x - uvMin.x) + uvMin.x, base.y * (uvMax.y - uvMin.y) + uvMin.y);
}

vec2 uv_aa_smoothstep(vec2 uv) {
    const vec2 res = vec2(4096, 4096);
    const float width = 1.5;
    vec2 pixels = uv * res;

    vec2 pixels_floor = floor(pixels + 0.5);
    vec2 pixels_fract = fract(pixels + 0.5);
    vec2 pixels_aa = fwidth(pixels) * width * 0.5;
    pixels_fract = smoothstep( vec2(0.5) - pixels_aa, vec2(0.5) + pixels_aa, pixels_fract );

    return (pixels_floor + pixels_fract - 0.5) / res;
}

void main() {
    vec2 uv = map(input1.BaseUV, input1.UV.xy, input1.UV.xy + input1.UV.zw);
    vec4 color = texture(GameTexture, uv_aa_smoothstep(uv));
    
    color /= color.a;
    color.rgb -= input1.Extra.z * 0.241 * clamp(0.6 - input1.Zed, 0.0 , 0.6);
    
    FragColor = color;
}