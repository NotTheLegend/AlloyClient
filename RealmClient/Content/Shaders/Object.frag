#version 330

in VS_OUT {
    vec2 BaseUV;
    vec4 UV;
    vec4 Extra;
    vec4 Color;
    vec4 Mask1;
    vec4 Mask2;
    float Depth;
} input;

out vec4 FragColor;

uniform sampler2D GameTexture;
uniform float PixelRange;
uniform vec2 TextTextureSize;
uniform sampler2D TextTexture;

const float TypeGameObject = 0.0;
const float TypeModel = 1.0;
const float TypeWall = 2.0;
const float TypeText = 3.0;
const float TypeBar = 4.0;

float map(float value, float newMin, float newMax) {
    return value * (newMax - newMin) + newMin;
}

vec2 map(vec2 values, vec2 newMins, vec2 newMaxs) {
    return vec2(map(values.x, newMins.x, newMaxs.x), map(values.y, newMins.y, newMaxs.y));
}

float median(float a, float b, float c) {
    return max(min(a, b), min(max(a, b), c));
}

vec2 uv_aa_smoothstep( vec2 uv,float width ) {
    const vec2 res = vec2(4096, 4096);
    vec2 pixels = uv * res;

    vec2 pixels_floor = floor(pixels + 0.5);
    vec2 pixels_fract = fract(pixels + 0.5);
    vec2 pixels_aa = fwidth(pixels) * width * 0.5;
    pixels_fract = smoothstep( vec2(0.5) - pixels_aa, vec2(0.5) + pixels_aa, pixels_fract );

    return (pixels_floor + pixels_fract - 0.5) / res;
}

float samp(vec2 uv, float width, vec2 dx, vec2 dy) {
    vec2 uv1 = clamp(uv, input.UV.xy, input.UV.xy + input.UV.zw);
    return textureGrad(GameTexture, uv_aa_smoothstep(uv1, 1.5), dx, dy).a;
}

vec4 GetModel() {
    vec2 uv = map(input.BaseUV, input.UV.xy, input.UV.xy + input.UV.zw);
    vec4 color = texture(GameTexture, uv_aa_smoothstep(uv, 1.5));
    color /= color.a;

    if (input.BaseUV.y > 0.4) {
        color.rgb -= input.Extra.z * 0.241 * (input.BaseUV.y - 0.4);
    }
    
    return color;
}

vec4 GetGameObject() {
    vec2 uv = map(input.BaseUV, input.UV.xy, input.UV.xy + input.UV.zw);
    vec2 dx = dFdx(uv);
    vec2 dy = dFdy(uv);
    vec4 color = textureGrad(GameTexture, uv_aa_smoothstep(uv, 1.5), dx, dy);
    color /= color.a;

    if (input.BaseUV.y > 0.4) {
        color.rgb -= input.Extra.z * 0.241 * (input.BaseUV.y - 0.4);
    }
    
    if (color.a > 0) {
        return color;
    }
    
    const float offset = 1.0 / 3.0 / 4096.0;
    const float val = 36.0 / 255.0 / 4096.0;
    float scaleX = length(dx) / val;
    float scaleY = length(dy) / val;
    const float width = 1.5;
    
    float alpha = max(0.0, samp(uv + vec2(offset * scaleX, -offset * scaleY), width, dx, dy));
    alpha = max(alpha, samp(uv + vec2(offset * scaleX, offset * scaleY), width, dx, dy));
    alpha = max(alpha, samp(uv + vec2(-offset * scaleX, -offset * scaleY), width, dx, dy));
    alpha = max(alpha, samp(uv + vec2(-offset * scaleX, offset * scaleY), width, dx, dy));
    
    alpha = max(alpha, samp(uv + vec2(offset * scaleX, 0), width, dx, dy));
    alpha = max(alpha, samp(uv + vec2(-offset * scaleX, 0), width, dx, dy));
    alpha = max(alpha, samp(uv + vec2(0, offset * scaleY), width, dx, dy));
    alpha = max(alpha, samp(uv + vec2(0, -offset * scaleY), width, dx, dy));
    
    if (alpha > 0) {
        return vec4(input.Color.rgb, 1);
    }

    float sum = 0.0;
    
    for (float x = -2; x <= 2; x++) {
        for (float y = -2.5; y <= 3.5; y++) {
            sum += samp(uv + vec2(x * offset, y * offset), 1.5, dx, dy);
        }
    }
    
    if (sum == 0.0)
        discard;
     
    return vec4(input.Color.rgb, sum / 30.0);
}

vec4 GetText() {
    vec2 uv = map(input.BaseUV, input.UV.xy, input.UV.xy + input.UV.zw);
    vec3 samp = texture(TextTexture, uv).rgb;
    float pRange = PixelRange;
    vec2 dim = TextTextureSize;

    vec2 msdfUnit = pRange / dim;
    float sigDist = median(samp.r, samp.g, samp.b) - 0.5f;
    sigDist = sigDist * dot(msdfUnit, 0.5f / fwidth(uv));
    const float strokeThickness = 0.250f * 0.75f;
    float strokeDist = median(samp.r, samp.g, samp.b) - 0.25f * (1.0 + (pRange - 12) / pRange) - strokeThickness;
    strokeDist = -(abs(strokeDist) - strokeThickness);
    strokeDist = strokeDist * dot(msdfUnit, 0.5f / fwidth(uv));
    float opacity = clamp(sigDist + 0.5f, 0.0f, 1.0f);
    float strokeOpacity = clamp(strokeDist + 0.5f, 0.0f, 1.0f);
    return mix(vec4(0, 0, 0, 1), input.Color, opacity) * max(opacity, strokeOpacity);
}

void main() {
    vec4 outputColor;
    float id = input.Extra.x;

    if (id == TypeGameObject) {
        outputColor = GetGameObject();
    } else if (id == TypeModel || id == TypeWall) {
        outputColor = GetModel();
    } else if (id == TypeText) {
        outputColor = GetText();
    } else if (id == TypeBar) {
        outputColor = input.Color;
    } else {
        outputColor = vec4(0, 0, 0, 0);
    }

    gl_FragDepth = input.Depth;

    outputColor.a *= input.Extra.w;
    if(outputColor.a == 0) {
        discard;
    }

    FragColor = outputColor;
}