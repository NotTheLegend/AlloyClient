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
float4x4 BillMatrix;

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
    float3 Position : TEXCOORD0;
	float4 Color : TEXCOORD1;
};

struct VertexOutput {
	float4 Position : POSITION0;
	float4 Color : TEXCOORD0;
	float2 BaseUV : TEXCOORD1;
	float Depth : TEXCOORD2;
};

struct PixelOut {
    float4 Color : SV_Target;
    float Depth : SV_Depth;
};

VertexOutput MainVertexShader(VertexInput input, VertexData data) {
    VertexOutput output = (VertexOutput)0;
    
    float4x4 billboard = BillMatrix;
    billboard._41 = data.Position.x;
    billboard._42 = data.Position.y;
    billboard._43 = data.Position.z;
            
    float4 billPos = mul(input.Position, billboard);
    float4 worldPos = mul(billPos, WorldMatrix);
    float4 viewPos = mul(worldPos, ViewMatrix);
    output.Position = mul(viewPos, ProjMatrix);
    output.Color = data.Color;
    
    float4 worldDepth = mul(float4(data.Position.x, data.Position.y, 0, 1), WorldMatrix);
    float4 viewDepth = mul(worldDepth, ViewMatrix);
    float4 projDepth = mul(viewDepth, ProjMatrix);
    output.Depth = 0.5f + 0.4f * projDepth.y;
    output.BaseUV = input.BaseUV;
    
    return output;
}

PixelOut MainPixelShader(VertexOutput input) {
    PixelOut output;
    
    output.Depth = input.Depth;
    
    if (input.Color.w > -1) {
        output.Color = tex2D(GameAtlasSample, input.Color.xy);
    } else {
        output.Color = float4(input.Color.x, input.Color.y, input.Color.z, 1);
    }
    
    if (input.BaseUV.x < 0.1 || input.BaseUV.x > 0.9)
        output.Color = float4(0, 0, 0, 1);
    if (input.BaseUV.y < 0.1 || input.BaseUV.y > 0.9)
        output.Color = float4(0, 0, 0, 1);
    
    return output;
}

technique Particles
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVertexShader();
		PixelShader = compile PS_SHADERMODEL MainPixelShader();
	}
};