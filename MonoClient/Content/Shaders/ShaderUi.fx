#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix ViewMatrix;

texture GameAtlasTexture;
sampler2D GameAtlasSample = sampler_state{
    Texture = (GameAtlasTexture);
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;

};

texture UiAtlasTexture;
sampler2D UiAtlasSample = sampler_state{
    Texture = (UiAtlasTexture);
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;

};

texture MinimapTexture;
sampler2D MinimapSample = sampler_state{
    Texture = (MinimapTexture);
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;

};

float PixelRange;
float2 TextTextureSize;
texture TextTexture;
sampler2D TextSample = sampler_state{
    Texture = (TextTexture);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;

};

float PixelRangeBold;
float2 TextTextureSizeBold;
texture TextBoldTexture;
sampler2D TextBoldSample = sampler_state{
    Texture = (TextBoldTexture);
    AddressU = CLAMP;
    AddressV = CLAMP;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    Mipfilter = LINEAR;

};

texture TitleBackgroundTexture;
sampler2D TitleBackgroundSample = sampler_state{
    Texture = (TitleBackgroundTexture);
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;

};

texture TitleGraphicTexture;
sampler2D TitleGraphicSample = sampler_state{
    Texture = (TitleGraphicTexture);
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;

};

struct VertexInput
{
    float4 Position : SV_POSITION0;
    float4 Color : COLOR0;
    float4 Override : COLOR1;
    float2 Info : TEXCOORD0;
    float2 UVCoords : TEXCOORD1;
    float4 Scissor : TEXCOORD2;
    float4 Extra1 : TEXCOORD3;
    float4 Extra2 : TEXCOORD4;
    float4 ColorTransform : TEXCOORD5;
};

struct VertexOutput
{
    float4 Position : SV_POSITION0;
    float4 Position1 : TEXCOORD6;
    float4 Color : COLOR0;
    float4 Override : COLOR1;
    float2 Info : TEXCOORD0;
    float2 UVCoords : TEXCOORD1;
    float4 Scissor : TEXCOORD2;
    float4 Extra1 : TEXCOORD3;
    float4 Extra2 : TEXCOORD4;
    float4 ColorTransform : TEXCOORD5;
};

VertexOutput MainVertex(in VertexInput input)
{
    VertexOutput output = (VertexOutput)0;
    output.Position = mul(input.Position, ViewMatrix);
    output.Position1 = output.Position;
    output.Color = input.Color;
    output.Override = input.Override;
    output.Info = input.Info;
    output.UVCoords = input.UVCoords;
    output.Scissor.xy = mul(float4(input.Scissor.x, input.Scissor.y, input.Position.z, input.Position.w), ViewMatrix).xy;
    output.Scissor.zw = mul(float4(input.Scissor.z, input.Scissor.w, input.Position.z, input.Position.w), ViewMatrix).xy;
    output.Extra1 = input.Extra1;
    output.Extra2 = input.Extra2;
    output.ColorTransform = input.ColorTransform;
    return output;
}

static const float TextTypeNormal = 0.0;
static const float TextTypeSmall = 1.0;

static const float IdColor = 0.0;
static const float IdGameAtlas = 1.0;
static const float IdUiAtlas = 2.0;
static const float IdUiSlice = 3.0;
static const float IdText = 4.0;
static const float IdTextBold = 5.0;
static const float IdTitleBackground = 6.0;
static const float IdTitleGraphic = 7.0;
static const float IdMinimap = 8.0;
static const float IdEllipse = 9.0;

float map(float value, float originalMin, float originalMax, float newMin, float newMax) {
    return (value - originalMin) / (originalMax - originalMin) * (newMax - newMin) + newMin;
}

float scale(float val, float2 rect, float border, float borderTex) {
    if (val <= border)
        return map(val, 0, border, rect.x, rect.x + borderTex);
    if (val >= 1.0 - border)
        return map(val, 1.0 - border, 1, rect.y - borderTex, rect.y);
    return map(val, border, 1.0 - border, rect.x + borderTex, rect.y - borderTex);
}

float4 slice(VertexOutput input) {
    float2 uv;
    uv.x = scale(input.UVCoords.x, input.Extra1.xy, input.Extra2.z, input.Extra2.x);
    uv.y = scale(input.UVCoords.y, input.Extra1.zw, input.Extra2.w, input.Extra2.y);
    return tex2D(UiAtlasSample, uv);
}

float median(float a, float b, float c) {
    return max(min(a, b), min(max(a, b), c));
}

float2 SafeNormalize(float2 v)
{
    float vLength = length(v);

    vLength = (vLength > 0.0) ? 1.0 / vLength : 0.0;

    return v * vLength;
}

float GetOpacityFromDistance(float signedDistance, float2 Jdx, float2 Jdy, float pRange) {
    const float distanceLimit = sqrt(2.0f) / 2.0f;  
    const float thickness = 1.0f / (pRange/ 2.0);
 
    float2 gradientDistance = SafeNormalize(float2(ddx(signedDistance), ddy(signedDistance)));
    float2 gradient = float2(gradientDistance.x * Jdx.x + gradientDistance.y * Jdy.x, gradientDistance.x * Jdx.y + gradientDistance.y * Jdy.y);
 
    float scaledDistanceLimit = min(thickness * distanceLimit * length(gradient), 0.5f);
 
    return smoothstep(-scaledDistanceLimit, scaledDistanceLimit, signedDistance);
}

float4 RenderText(VertexOutput input, sampler2D sample, float2 dim, float pRange) {
    float2 msdfUnit = pRange / dim;
    float3 samp = tex2D(sample, input.UVCoords).rgb;
    float4 outColor = pRange == 0.0 ? input.Color : input.Override;

    float sigDist = median(samp.r, samp.g, samp.b) - 0.5f;
    sigDist = sigDist * dot(msdfUnit, 0.5f / fwidth(input.UVCoords));
    const float strokeThickness = 0.250f * 0.75f;
    float strokeDist = median(samp.r, samp.g, samp.b) - 0.25f * (1.0 + (pRange - input.Extra1.x) / pRange) - strokeThickness;
    strokeDist = -(abs(strokeDist) - strokeThickness);
    strokeDist = strokeDist * dot(msdfUnit, 0.5f / fwidth(input.UVCoords));
    
    float opacity = 0;
    float strokeOpacity = 0;
    
    if (input.Extra1.y == TextTypeSmall) {
        float2 pixelCoord = input.UVCoords * dim;
        float2 Jdx = ddx(pixelCoord);
        float2 Jdy = ddy(pixelCoord);
        opacity = GetOpacityFromDistance(sigDist, Jdx, Jdy, pRange);
        strokeOpacity = GetOpacityFromDistance(strokeDist, Jdx, Jdy, pRange);
    } else {
        opacity = clamp(sigDist + 0.5f, 0.0f, 1.0f);
        strokeOpacity = clamp(strokeDist + 0.5f, 0.0f, 1.0f);
    }
    
    return lerp(outColor, input.Color, opacity) * max(opacity, strokeOpacity);
}

float4 RenderOutline(VertexOutput input, sampler2D sample) : COLOR {
    float4 color = tex2D(sample, input.UVCoords);
    
    if (input.UVCoords.y > input.Extra1.x) {
        color.rgb += 0.9241 * (input.UVCoords.y - input.Extra1.x) / input.Extra1.y;
    }
    
    if (color.a > 0) {
        return color;
    }
    
    float2 offsetX = {1.0 / 4.0 / 4096.0, 0};
    float2 offsetY = {0, 1.0 / 4.0 / 4096.0};
    float alpha = 0;
    float2 c;
    float2 uv = input.UVCoords;

    c = uv + offsetX - offsetY;
    alpha = max(alpha, tex2D(GameAtlasSample, c).a);
    c = uv + offsetX + offsetY;
    alpha = max(alpha, tex2D(GameAtlasSample, c).a);
    c = uv - offsetX - offsetY;
    alpha = max(alpha, tex2D(GameAtlasSample, c).a);
    c = uv - offsetX + offsetY;
    alpha = max(alpha, tex2D(GameAtlasSample, c).a);


    if (alpha > 0) {
        color = input.Override;
    }   
    
    return color;
}

float4 RenderNoOutline(VertexOutput input, sampler2D sample) : COLOR {
    return tex2D(sample, input.UVCoords);
}

float4 RenderMinimap(VertexOutput input) : COLOR {
    float2 coords = input.UVCoords;

    if (coords.x < 0 || coords.x > 1 || coords.y < 0 || coords.y > 1) {
        return float4(0, 0, 0, 1);
    }

    return tex2D(MinimapSample, coords);    
}

float4 RenderEllipse(VertexOutput input) : COLOR {
    float rx = input.Extra1.x - input.Extra1.z, ry = input.Extra1.y - input.Extra1.z;
    float x = input.UVCoords.x, y = input.UVCoords.y;
    
    float inner = x * x / (rx * rx) + y * y / (ry * ry);
    
    rx = input.Extra1.x, ry = input.Extra1.y;
    float outline = x * x / (rx * rx) + y * y / (ry * ry);
    
    if (x * x / (rx * rx) + y * y / (ry * ry) > 1) 
        return float4(0, 0, 0, 0);
        
    float color;
    
    if (inner > 1) {
        color = 1;
    } else {
        color = 0; 
    }
    
    return lerp(input.Color, input.Override, color); 
}

float4 MainPixel(VertexOutput input) : COLOR {
    float4 pixel = (float4)0;

    if (input.Position1.x < input.Scissor.x || input.Position1.x > input.Scissor.z || input.Position1.y < input.Scissor.w || input.Position1.y > input.Scissor.y) {
        discard;
    }

    float type = input.Info.x;

    if (type == IdColor) {
        pixel = input.Color;
    } else if (type == IdGameAtlas) {
        pixel = RenderOutline(input, GameAtlasSample);
    } else if (type == IdUiAtlas) {
        pixel = RenderNoOutline(input, UiAtlasSample);
    } else if (type == IdUiSlice){
        pixel = slice(input);
    } else if (type == IdText) {
        pixel = RenderText(input, TextSample, TextTextureSize, PixelRange);
    } else if (type == IdTextBold) {
        pixel = RenderText(input, TextBoldSample, TextTextureSizeBold, PixelRangeBold);
    } else if (type == IdTitleBackground) {
        pixel = RenderNoOutline(input, TitleBackgroundSample);
    } else if (type == IdTitleGraphic) {
        pixel = RenderNoOutline(input, TitleGraphicSample);
    } else if (type == IdMinimap) {
        pixel = RenderMinimap(input);
    } else if (type == IdEllipse) {
        pixel = RenderEllipse(input);
    }

    if (input.Color.a > 0 && type != IdColor && type != IdText && type != IdTextBold && type != IdEllipse)
        pixel *= input.Color;

    int4 add = input.ColorTransform / 1000;
    float4 mult = input.ColorTransform - add * 1000;
    
    pixel = clamp(pixel, float4(0, 0, 0, 0), float4(1, 1, 1, 1));
    
    pixel = mult * pixel;
    pixel += add / 255.0;

    pixel.a *= input.Info.y;
    return pixel;
}

technique MainDraw {
	pass P0 {
        AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        VertexShader = compile VS_SHADERMODEL MainVertex();
        PixelShader = compile PS_SHADERMODEL MainPixel();
    }
};
