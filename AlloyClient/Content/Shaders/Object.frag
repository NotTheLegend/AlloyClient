#version 460 core

struct extra {
    float Type;
    float SortId;
    float Shade;
    float Alpha;
};

in OBJECT_OUT {
    vec2 BaseUV;
    vec4 UV;
    extra Extra;
    vec4 Color;
    vec4 Mask1;
    vec4 Mask2;
} vsInput;

out vec4 FragColor;

uniform sampler2D GameTexture;
uniform float PixelRange;
uniform vec2 TextTextureSize;
uniform sampler2D TextTexture;

const float TypeGameObject = 0.0;
const float TypeText = 3.0;
const float TypeBar = 4.0;
const float TypeEffect = 5.0;

vec2 map(vec2 base, vec2 uvMin, vec2 uvMax) {
    return vec2(base.x * (uvMax.x - uvMin.x) + uvMin.x, base.y * (uvMax.y - uvMin.y) + uvMin.y);
}

float median(float a, float b, float c) {
    return max(min(a, b), min(max(a, b), c));
}

float samp(vec2 uv, vec2 dx, vec2 dy) {
    return textureGrad(GameTexture, uv, dx, dy).a;
}

vec4 GetGameObject() {
    vec2 uv = map(vsInput.BaseUV, vsInput.UV.xy, vsInput.UV.xy + vsInput.UV.zw);
    vec2 dx = dFdx(uv);
    vec2 dy = dFdy(uv);
    vec4 color = textureGrad(GameTexture, uv, dx, dy);
    color.rgb -= vsInput.Extra.Shade * 0.241 * clamp(0.4 - vsInput.BaseUV.y, 0.0 , 0.4);

    if (color.a > 0) {
        return color;
    }

    float offX = length(dx);
    float offY = length(dy);
    
    float alpha = max(0.0, samp(uv + vec2(offX, -offY), dx, dy));
    alpha = max(alpha, samp(uv + vec2(offX, offY), dx, dy));
    alpha = max(alpha, samp(uv + vec2(-offX, -offY), dx, dy));
    alpha = max(alpha, samp(uv + vec2(-offX, offY), dx, dy));

    if (alpha > 0) {
        return vec4(vsInput.Color.rgb, 1);
    }
    
    discard;
}

vec4 GetText() {
    vec2 uv = map(vsInput.BaseUV, vsInput.UV.xy, vsInput.UV.xy + vsInput.UV.zw);
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
    return mix(vec4(0, 0, 0, 1), vsInput.Color, opacity) * max(opacity, strokeOpacity);
}

void main() {
    vec4 outputColor;
    float id = vsInput.Extra.Type;

    if (id == TypeGameObject || id == TypeEffect) {
        outputColor = GetGameObject();
    } else if (id == TypeText) {
        outputColor = GetText();
    } else if (id == TypeBar) {
        outputColor = vsInput.Color;
    } else {
        outputColor = vec4(0, 0, 0, 0);
    }

    outputColor.a *= vsInput.Extra.Alpha;
    if(outputColor.a == 0) {
        discard;
    }

    FragColor = outputColor;
}