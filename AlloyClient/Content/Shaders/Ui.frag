#version 330

in VS_OUT {
    vec4 Position1;
    flat uint Color;
    flat uint Override;
    vec2 Info;
    vec2 UVCoords;
    vec4 Scissor;
    vec4 Extra1;
    vec4 Extra2;
    vec4 ColorTransform;
} input;

out vec4 FragColor;

uniform sampler2D GameAtlasTexture;
uniform sampler2D UiAtlasTexture;
uniform sampler2D UiAtlasTextureLinear;
uniform sampler2D MinimapTexture;

uniform float PixelRange;
uniform vec2 TextTextureSize;
uniform sampler2D TextTexture;

uniform sampler2D TitleBackgroundTexture;
uniform sampler2D TitleGraphicTexture;


const float TextTypeNormal = 0.0;
const float TextTypeSmall = 1.0;

const float IdColor = 0.0;
const float IdGameAtlas = 1.0;
const float IdUiAtlas = 2.0;
const float IdUiAtlasLinear = 3.0;
const float IdUiSlice = 4.0;
const float IdText = 5.0;
const float IdTitleBackground =6.0;
const float IdTitleGraphic = 7.0;
const float IdMinimap = 8.0;
const float IdEllipse = 9.0;

vec4 unpackColor(uint color) {
    return vec4(
    float(color & 0x000000FFu) / 255.0,
    float((color & 0x0000FF00u) >> 8u) / 255.0,
    float((color & 0x00FF0000u) >> 16u) / 255.0,
    float((color & 0xFF000000u) >> 24u) / 255.0
    );
}

float map(float value, float originalMin, float originalMax, float newMin, float newMax) {
    return (value - originalMin) / (originalMax - originalMin) * (newMax - newMin) + newMin;
}

float scale(float val, vec2 rect, float border, float borderTex) {
    if (val <= border)
    return map(val, 0, border, rect.x, rect.x + borderTex);
    if (val >= 1.0 - border)
    return map(val, 1.0 - border, 1, rect.y - borderTex, rect.y);
    return map(val, border, 1.0 - border, rect.x + borderTex, rect.y - borderTex);
}

vec4 slice() {
    vec2 uv;
    uv.x = scale(input.UVCoords.x, input.Extra1.xy, input.Extra2.z, input.Extra2.x);
    uv.y = scale(input.UVCoords.y, input.Extra1.zw, input.Extra2.w, input.Extra2.y);
    return texture(UiAtlasTexture, uv);
}

float median(float a, float b, float c) {
    return max(min(a, b), min(max(a, b), c));
}

float screenPxRange(vec2 uv) {
    vec2 unitRange = vec2(PixelRange, PixelRange) / TextTextureSize;
    vec2 screenSize = vec2(1.0, 1.0) / fwidth(uv);
    return max(0.5 * dot(unitRange, screenSize), 1.0);
}

vec2 SafeNormalize(vec2 v) {
    float vLength = length(v);

    vLength = (vLength > 0.0) ?
    1.0 / vLength : 0.0;

    return v * vLength;
}

float GetOpacityFromDistance(float signedDistance, vec2 Jdx, vec2 Jdy) {
    const float distanceLimit = sqrt(2.0f) / 2.0f;
    float thickness = 1.0f / (PixelRange / 2.0);

    vec2 gradientDistance = SafeNormalize(vec2(dFdx(signedDistance), dFdy(signedDistance)));
    vec2 gradient = vec2(gradientDistance.x * Jdx.x + gradientDistance.y * Jdy.x, gradientDistance.x * Jdx.y + gradientDistance.y * Jdy.y);
    float scaledDistanceLimit = min(thickness * distanceLimit * length(gradient), 0.5f);

    return smoothstep(-scaledDistanceLimit, scaledDistanceLimit, signedDistance);
}

vec4 RenderText() {
    vec4 mtsdf = texture(TextTexture, input.UVCoords);
    float dist = median(mtsdf.r, mtsdf.g, mtsdf.b) - 0.5;
    float pxRange = screenPxRange(input.UVCoords);

    float bodyDist = dist * pxRange;
    float glowDist = mtsdf.a;
    float glowSize = input.Extra1.x / PixelRange;
    float bodyAlpha;
    float glowAlpha;

    if (input.Extra1.y == TextTypeSmall) {
        vec2 pixelCoord = input.UVCoords * TextTextureSize;
        vec2 Jdx = dFdx(pixelCoord);
        vec2 Jdy = dFdy(pixelCoord);
        bodyAlpha = GetOpacityFromDistance(bodyDist, Jdx, Jdy);
        glowAlpha = GetOpacityFromDistance(glowDist, Jdx, Jdy) * glowSize;
    } else {
        bodyAlpha = clamp(bodyDist + 0.5f, 0.0f, 1.0f);
        glowAlpha = glowDist * glowSize;
    }

    vec4 color = mix(unpackColor(input.Override), unpackColor(input.Color), bodyAlpha);
    float alpha = bodyAlpha + glowAlpha;
    return vec4(color.rgb, alpha);
}

float samp(vec2 uv, vec2 dx, vec2 dy) {
    return textureGrad(GameAtlasTexture, uv, dx, dy).a;
}

vec4 RenderOutline() {
    vec2 uv = input.UVCoords;
    vec2 dx = dFdx(uv);
    vec2 dy = dFdy(uv);
    vec4 color = textureGrad(GameAtlasTexture, uv, dx, dy);
    
    if (input.UVCoords.y > input.Extra1.x) {
        color.rgb -= 0.241 * (((input.UVCoords.y - input.Extra1.y) / input.Extra1.z) - 0.4);
    }

    if (color.a > 0) {
        return color;
    }

    if (input.Extra1.w == -1)
        discard;

    float offX = length(dx);
    float offY = length(dy);

    float alpha = max(0.0, samp(uv + vec2(offX, -offY), dx, dy));
    alpha = max(alpha, samp(uv + vec2(offX, offY), dx, dy));
    alpha = max(alpha, samp(uv + vec2(-offX, -offY), dx, dy));
    alpha = max(alpha, samp(uv + vec2(-offX, offY), dx, dy));

    vec4 outline = unpackColor(input.Override);

    if (alpha > 0) {
        return vec4(outline.rgb, 1);
    }

    discard;
}

vec4 RenderNoOutline(sampler2D tex) {
    return texture(tex, input.UVCoords);
}

vec4 RenderMinimap() {
    vec2 coords = input.UVCoords;
    if (coords.x < 0 || coords.x > 1 || coords.y < 0 || coords.y > 1) {
        return vec4(0, 0, 0, 1);
    }

    return texture(MinimapTexture, coords);
}

vec4 RenderEllipse() {
    float rx = input.Extra1.x - input.Extra1.z, ry = input.Extra1.y - input.Extra1.z;
    float x = input.UVCoords.x, y = input.UVCoords.y;

    float inner = x * x / (rx * rx) + y * y / (ry * ry);
    rx = input.Extra1.x; ry = input.Extra1.y;
    float outline = x * x / (rx * rx) + y * y / (ry * ry);
    if (x * x / (rx * rx) + y * y / (ry * ry) > 1)
        return vec4(0, 0, 0, 0);
    float color_val;

    if (inner > 1) {
        color_val = 1;
    } else {
        color_val = 0;
    }

    return mix(unpackColor(input.Color), unpackColor(input.Override), color_val);
}

void main() {
    vec4 pixel = vec4(0);
    
    
    //TODO: replace pos1 with gl_FragCoord and send screen coords in scissor instead
    if (input.Position1.x < input.Scissor.x || input.Position1.x > input.Scissor.z || input.Position1.y < input.Scissor.w || input.Position1.y > input.Scissor.y) {
        discard;
    }
    
    vec4 color = unpackColor(input.Color);

    float type = input.Info.x;

    if (type == IdColor) {
        pixel = color;
    } else if (type == IdGameAtlas) {
        pixel = RenderOutline();
    } else if (type == IdUiAtlas) {
        pixel = RenderNoOutline(UiAtlasTexture);
    } else if (type == IdUiAtlasLinear) {
        pixel = RenderNoOutline(UiAtlasTextureLinear);// todo: msdfa sampling
    } else if (type == IdUiSlice) {
        pixel = slice();
    } else if (type == IdText) {
        pixel = RenderText();
    } else if (type == IdTitleBackground) {
        pixel = RenderNoOutline(TitleBackgroundTexture);
    } else if (type == IdTitleGraphic) {
        pixel = RenderNoOutline(TitleGraphicTexture);
    } else if (type == IdMinimap) {
        pixel = RenderMinimap();
    } else if (type == IdEllipse) {
        pixel = RenderEllipse();
    }

    if (color.a > 0 && type != IdColor && type != IdText && type != IdEllipse)
        pixel *= color;
    
    vec4 add = floor(input.ColorTransform / 1000.0);
    vec4 mult = input.ColorTransform - add * 1000.0;

    pixel = clamp(pixel, vec4(0.0), vec4(1.0));

    pixel = mult * pixel;
    pixel += add / 255.0;

    pixel.a *= input.Info.y;
    FragColor = pixel;
}