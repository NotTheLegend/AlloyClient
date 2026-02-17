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
uniform float Zoom;

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

bool inBounds(vec2 uv, vec2 minUV, vec2 maxUV){
    if (uv.x < minUV.x ||
    uv.x > maxUV.x ||
    uv.y < minUV.y ||
    uv.y > maxUV.y)
    {
        return false;
    }
    return true;
}

vec4 GetGameObject() {
    vec2 uv = map(vsInput.BaseUV, vsInput.UV.xy, vsInput.UV.xy + vsInput.UV.zw);
    vec2 dx = dFdx(uv);
    vec2 dy = dFdy(uv);
    vec4 color = textureGrad(GameTexture, uv, dx, dy);
    color.rgb -= vsInput.Extra.Shade * 0.241 * clamp(vsInput.BaseUV.y - 0.4, 0.0, 0.4);

    if (color.a > 0) {
        return color;
    }

    vec2 minUV = vsInput.UV.xy;
    vec2 maxUV = vsInput.UV.xy + vsInput.UV.zw;

    float offX = length(dx);
    float offY = length(dy);

    float outlineSize = max(1, Zoom); // Outline params
    float outlineAlpha = 0.0;
    
    float glowAlpha = 0.0; // Glow params
    float glowSize = max(6.0, Zoom * 6.0);
    float maxDist = glowSize * max(offX, offY);

    // Draw outline & glow
    for (float i = 1; i <= glowSize; i++) {
        float ox = offX * i;
        float oy = offY * i;
        vec2 offsets[8] = vec2[](
            vec2(-ox, -oy), // Top-left
            vec2(0.0, -oy), // Top-middle
            vec2(ox, -oy), // Top-right
            vec2(ox, 0.0), // Right-middle
            vec2(ox, oy), // Bottom-right
            vec2(0.0, oy), // Bottom-middle
            vec2(-ox, oy), // Bottom-left
            vec2(-ox, 0.0)// Left-middle
        );
        
        for (int j = 0; j < 8; j++) {
            vec2 sampleUV = uv + offsets[j];
            if (!inBounds(sampleUV, minUV, maxUV)) {
                continue;
            }

            float a = samp(sampleUV, dx, dy);
            if (a > 0){
                float dist = length(offsets[j]);
                float normalized = dist / maxDist;

                float alpha = 0.8 * exp(-normalized * 4.0);

                glowAlpha = max(glowAlpha, alpha);
            }
            
            if (i <= outlineSize){
                outlineAlpha = max(outlineAlpha, a);
            }
        }
    }

    if (outlineAlpha > 0.0) {
        return vec4(vsInput.Color.rgb, 1.0);
    }

    if (glowAlpha > 0.0) {
        return vec4(vsInput.Color.rgb, glowAlpha);
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
    if (outputColor.a == 0) {
        discard;
    }

    FragColor = outputColor;
}