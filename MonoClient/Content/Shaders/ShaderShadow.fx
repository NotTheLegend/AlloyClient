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

struct VertexInput {
	float4 Position : POSITION0;
	float2 BaseUV : BLENDWEIGHT0;
};

struct VertexData {
    float3 Position : TEXCOORD0;
    float2 Scale : TEXCOORD1;
    float4 Color : TEXCOORD2;
};

struct VertexOutput {
	float4 Position : SV_POSITION;
	float2 BaseUV : TEXCOORD0;
    float4 Color : TEXCOORD1;
};

VertexOutput MainVertexShader(VertexInput input, VertexData data)
{
    VertexOutput output;
    input.Position.xy *= data.Scale;
    input.Position.xyz += data.Position.xyz;
    float4 worldPos = mul(input.Position, WorldMatrix);
    float4 viewPos = mul(worldPos, ViewMatrix);
    output.Position = mul(viewPos, ProjMatrix);
    output.BaseUV = input.BaseUV;
    output.Color = data.Color;
    return output;
}

float4 MainPixelShader(VertexOutput input) : COLOR {
    float dx = 0.5 - input.BaseUV.x, dy = 0.5 - input.BaseUV.y;
    float dist = dx * dx + dy * dy;
    float distFromCenter = 0.25 - dist;
    return float4(input.Color.r, input.Color.g, input.Color.b, distFromCenter * 1.5);
}

technique Shadow {
	pass p0 {
        AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
        VertexShader = compile VS_SHADERMODEL MainVertexShader();
        PixelShader = compile PS_SHADERMODEL MainPixelShader();
    
    }
};