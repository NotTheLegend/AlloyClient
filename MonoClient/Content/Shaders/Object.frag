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

vec4 GetGameObject() {
    vec2 uv = map(input.BaseUV, input.UV.xy, input.UV.xy + input.UV.zw);
    vec4 color = textureGrad(GameTexture, uv, dFdx(uv), dFdy(uv));

    if (input.BaseUV.y > 0.4) {
        color.rgb -= input.Extra.z * 0.241 * (input.BaseUV.y - 0.4);
    }

    if (color.a > 0) {
        color.a *= input.Extra.w;
        return color;
    }

    if (input.Extra.x != 0) {
        discard;
    }

    const vec2 offsetX = vec2(1.0 / 4.0 / 4096.0, 0);
    const vec2 offsetY = vec2(0, 1.0 / 4.0 / 4096.0);
    float alpha = 0;
    vec2 c;
    c = uv + offsetX - offsetY;
    alpha = max(alpha, texture(GameTexture, c).a);
    c = uv + offsetX + offsetY;
    alpha = max(alpha, texture(GameTexture, c).a);
    c = uv - offsetX - offsetY;
    alpha = max(alpha, texture(GameTexture, c).a);
    c = uv - offsetX + offsetY;
    alpha = max(alpha, texture(GameTexture, c).a);
    if (alpha > 0) {
        color = vec4(0, 0, 0, 1);
        color.a *= input.Extra.w;
        return color;
    }

    color = input.Color;
    c = uv;
    float sum = 0.0;

    const float px = 1.0 / 4.0 / 4096.0;
    const float py = 1.0 / 4.0 / 4096.0;
    float x2 = px * 2;
    float x3 = px * 3;

    float y3 = py * -3.5;
    sum += texture(GameTexture, c - vec2(x3, y3)).a;
    sum += texture(GameTexture, c - vec2(x2, y3)).a;
    sum += texture(GameTexture, c - vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(0.0, y3)).a;
    sum += texture(GameTexture, c + vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(x2, y3)).a;
    sum += texture(GameTexture, c + vec2(x3, y3)).a;

    y3 = py * -2.5;

    sum += texture(GameTexture, c - vec2(x3, y3)).a;
    sum += texture(GameTexture, c - vec2(x2, y3)).a;
    sum += texture(GameTexture, c - vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(0.0, y3)).a;
    sum += texture(GameTexture, c + vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(x2, y3)).a;
    sum += texture(GameTexture, c + vec2(x3, y3)).a;

    y3 = py * -1.5;
    sum += texture(GameTexture, c - vec2(x3, y3)).a;
    sum += texture(GameTexture, c - vec2(x2, y3)).a;
    sum += texture(GameTexture, c - vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(0.0, y3)).a;
    sum += texture(GameTexture, c + vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(x2, y3)).a;
    sum += texture(GameTexture, c + vec2(x3, y3)).a;

    y3 = py * -0.5;

    sum += texture(GameTexture, c - vec2(x3, y3)).a;
    sum += texture(GameTexture, c - vec2(x2, y3)).a;
    sum += texture(GameTexture, c - vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(0.0, y3)).a;
    sum += texture(GameTexture, c + vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(x2, y3)).a;
    sum += texture(GameTexture, c + vec2(x3, y3)).a;

    y3 = py * 0.5;
    sum += texture(GameTexture, c - vec2(x3, y3)).a;
    sum += texture(GameTexture, c - vec2(x2, y3)).a;
    sum += texture(GameTexture, c - vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(0.0, y3)).a;
    sum += texture(GameTexture, c + vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(x2, y3)).a;
    sum += texture(GameTexture, c + vec2(x3, y3)).a;

    y3 = py * 1.5;

    sum += texture(GameTexture, c - vec2(x3, y3)).a;
    sum += texture(GameTexture, c - vec2(x2, y3)).a;
    sum += texture(GameTexture, c - vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(0.0, y3)).a;
    sum += texture(GameTexture, c + vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(x2, y3)).a;
    sum += texture(GameTexture, c + vec2(x3, y3)).a;

    y3 = py * 2.5;
    sum += texture(GameTexture, c - vec2(x3, y3)).a;
    sum += texture(GameTexture, c - vec2(x2, y3)).a;
    sum += texture(GameTexture, c - vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(0.0, y3)).a;
    sum += texture(GameTexture, c + vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(x2, y3)).a;
    sum += texture(GameTexture, c + vec2(x3, y3)).a;

    y3 = py * 3.5;

    sum += texture(GameTexture, c - vec2(x3, y3)).a;
    sum += texture(GameTexture, c - vec2(x2, y3)).a;
    sum += texture(GameTexture, c - vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(0.0, y3)).a;
    sum += texture(GameTexture, c + vec2(px, y3)).a;
    sum += texture(GameTexture, c + vec2(x2, y3)).a;
    sum += texture(GameTexture, c + vec2(x3, y3)).a;

    color.a = sum / 49.0;
    return color;
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

    if (id == TypeGameObject || id == TypeModel || id == TypeWall) {
        outputColor = GetGameObject();
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