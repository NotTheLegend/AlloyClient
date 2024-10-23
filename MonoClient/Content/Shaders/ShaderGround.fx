#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 WorldMatrix;
float4x4 ViewMatrix;
float4x4 ProjMatrix;
float GameTime;
float4 AlphaBlends[8];

texture GameTexture;
sampler2D GameAtlasSample = sampler_state {
    Texture = (GameTexture);
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexInput {
	float4 Position : POSITION0;
	float2 BaseUV : BLENDWEIGHT0;
};

struct VertexData {
    float4 Position : TEXCOORD0;
	float4 UV : TEXCOORD1;
	float4 Animate : TEXCOORD2;
	float4 BlendLeftRight : TEXCOORD3;
	float4 BlendTopBottom : TEXCOORD4;
	float4 CornerBottom : TEXCOORD5;
	float4 CornerTop : TEXCOORD6;
};

struct VertexOutput {
	float4 Position : POSITION0;
	float2 BaseUV : TEXCOORD0;
	float4 UV : TEXCOORD1;
	float4 Animate : TEXCOORD2;
	float4 BlendLeftRight : TEXCOORD3;
	float4 BlendTopBottom : TEXCOORD4;
	float4 CornerBottom : TEXCOORD5;
    float4 CornerTop : TEXCOORD6;
    float2 CoreUV : TEXCOORD7;
};

static const float atlasPad = 1.0 / 6.0 / 4096;

float map(float value, float newMin, float newMax) {
    return abs((value + 1) % 1.0) * (newMax - newMin - atlasPad * 2) + newMin + atlasPad;
}

float2 map(float2 values, float2 newMins, float2 newMaxs) {
    return float2(map(values.x, newMins.x, newMaxs.x), map(values.y, newMins.y, newMaxs.y));
}

VertexOutput MainVertexShader(VertexInput input, VertexData data) {
    VertexOutput output = (VertexOutput)0;
    input.Position.xy += data.Position.xy;
    
    float4 worldPos = mul(input.Position, WorldMatrix);
    float4 viewPos = mul(worldPos, ViewMatrix);
    output.Position = mul(viewPos, ProjMatrix);
    
    output.BaseUV = input.BaseUV;
    
    output.CoreUV.x = input.BaseUV.x + data.Position.z + sin(GameTime * data.Animate.x) + GameTime * data.Animate.z;
    output.CoreUV.y = input.BaseUV.y + data.Position.w + sin(GameTime * data.Animate.y) + GameTime * data.Animate.w;
    
    output.UV = data.UV;
    output.Animate = data.Animate;
    output.BlendLeftRight = data.BlendLeftRight;
    output.BlendTopBottom = data.BlendTopBottom;
    output.CornerBottom = data.CornerBottom;
    output.CornerTop = data.CornerTop;
    //output.Offset = float2(data.Position.z + sin(GameTime * data.Animate.x) + GameTime * data.Animate.z, data.Position.w + sin(GameTime * data.Animate.y) + GameTime * data.Animate.w);
    return output;
}

float4 MainPixelShader(VertexOutput input) : COLOR {
    float2 ogCoords = map(input.CoreUV, input.UV.xy, input.UV.xy + input.UV.zw);
    float4 ogColor = tex2D(GameAtlasSample, ogCoords);
    float4 color = ogColor;
    
    float2 uv = input.UV.zw;
    float highAlpha = 0;
    float4 uva = (float4)0;
    float alpha = 0;
    float2 uvb = (float2)0;
    
    uvb = input.BlendLeftRight.xy;
    uva = AlphaBlends[0];
    alpha = tex2D(GameAtlasSample, map(input.BaseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = lerp(ogColor, tex2D(GameAtlasSample, map(input.CoreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }
    
    uvb = input.BlendLeftRight.zw;
    uva = AlphaBlends[1];
    alpha = tex2D(GameAtlasSample, map(input.BaseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = lerp(ogColor, tex2D(GameAtlasSample, map(input.CoreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }
    
    uvb = input.BlendTopBottom.zw;
    uva = AlphaBlends[2];
    alpha = tex2D(GameAtlasSample, map(input.BaseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = lerp(ogColor, tex2D(GameAtlasSample, map(input.CoreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }
    
    uvb = input.BlendTopBottom.xy;
    uva = AlphaBlends[3];
    alpha = tex2D(GameAtlasSample, map(input.BaseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = lerp(ogColor, tex2D(GameAtlasSample, map(input.CoreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }

    uvb = input.CornerBottom.xy;
    uva = AlphaBlends[4];
    alpha = tex2D(GameAtlasSample, map(input.BaseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = lerp(ogColor, tex2D(GameAtlasSample, map(input.CoreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }

    uvb = input.CornerBottom.zw;
    uva = AlphaBlends[5];
    alpha = tex2D(GameAtlasSample, map(input.BaseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = lerp(ogColor, tex2D(GameAtlasSample, map(input.CoreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }
    
    uvb = input.CornerTop.xy;
    uva = AlphaBlends[6];
    alpha = tex2D(GameAtlasSample, map(input.BaseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = lerp(ogColor, tex2D(GameAtlasSample, map(input.CoreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }
    
    uvb = input.CornerTop.zw;
    uva = AlphaBlends[7];
    alpha = tex2D(GameAtlasSample, map(input.BaseUV, uva.xy, uva.xy + uva.zw)).a;
    if (uvb.x >= 0 && alpha > 0.0 && alpha > highAlpha) {
        highAlpha = alpha;
        color = lerp(ogColor, tex2D(GameAtlasSample, map(input.CoreUV, uvb.xy, uvb.xy + uv)), highAlpha);
    }
        
    return color; 
}

technique GroundDraw
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVertexShader();
		PixelShader = compile PS_SHADERMODEL MainPixelShader();
	}
};