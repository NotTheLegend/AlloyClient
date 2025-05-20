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

struct VertexInput {
	float4 Position : POSITION0;
	float2 BaseUV : BLENDWEIGHT0;
};

struct VertexData {
    float4 Position : TEXCOORD0;
	float4 UV : TEXCOORD1;
	float4 Scale : TEXCOORD2;
	float4 Rotation : TEXCOORD3;
	float4 Extra : TEXCOORD4;
	float4 Color : COLOR0;
	float4 Mask1 : TEXCOORD6;
	float4 Mask2 : TEXCOORD7;
};

struct VertexOutput {
	float4 Position : POSITION0;
	float2 BaseUV : TEXCOORD0;
	float4 UV : TEXCOORD1;
	float4 Extra : TEXCOORD2;
	float4 Color : COLOR0;
    float4 Mask1 : TEXCOORD4;
    float4 Mask2 : TEXCOORD5;
};

struct PixelOut {
    float4 Color : SV_Target;
    float Depth : SV_Depth;
};

static const float TypeGameObject = 0.0;
static const float TypeModel = 1.0;
static const float TypeWall = 2.0;
static const float TypeText = 3.0;
static const float TypeBar = 4.0;

float4 GetPosition(float4 position, VertexData data) {
    float4 newPosition;
    float id = data.Extra.x;
    
    if (id == TypeGameObject || id == TypeText || id == TypeBar) {
        position.xy *= data.Scale.xy;
        
        float4 rot = data.Rotation;
        float4x4 rotate = {
            rot.y * rot.z, -rot.x * rot.z, 0, 0,
            rot.x * rot.z, rot.y * rot.z, 0, 0,
            0, 0, 1, 0,
            data.Scale.z * rot.z * -rot.w, data.Scale.w * rot.z, 0, 1
        };
        
        float4x4 billboard = BillMatrix;
        billboard._41 = data.Position.x;
        billboard._42 = data.Position.y;
        billboard._43 = data.Position.z;
        
        return mul(mul(position, rotate), billboard);        
    } else if (id == TypeModel){
        float s = sin(data.Rotation.x);
        float c = cos(data.Rotation.x);    
        
        float4x4 rotate = {
            c, s, 0, 0,
            -s, c, 0, 0,
            0, 0, 1, 0,
            data.Position.x, data.Position.y, data.Position.z, 1
        };
                
        return mul(position, rotate);
    } else if (id == 2) {
        float4x4 rm = {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            data.Position.x - 0.5, data.Position.y - 0.5, data.Position.z, 1
        };
        
        position.xyz += data.Position.xyz;
        position.xy -= 0.5;
                
        return position;       
    } else {
        position.xyz += data.Position.xyz;
        return position;
    }
}

float2 GetUV(float2 uv, VertexData data) {
    float id = data.Extra.x;

    if (id == TypeGameObject) {
        uv.x = 0.5 + (0.5 - uv.x) * data.Rotation.w;
    }
    
    return uv;
}

VertexOutput MainVertexShader(VertexInput input, VertexData data) {
    VertexOutput output = (VertexOutput)0;
    float4 worldPos = mul(GetPosition(input.Position, data), WorldMatrix);
    float4 viewPos = mul(worldPos, ViewMatrix);
    output.Position = mul(viewPos, ProjMatrix);
    output.BaseUV = GetUV(input.BaseUV, data);
    output.UV = data.UV;
    output.Extra = data.Extra;
    output.Color = data.Color;
    
    return output;
}

float map(float value, float newMin, float newMax) {
    return value * (newMax - newMin) + newMin;
}

float2 map(float2 values, float2 newMins, float2 newMaxs) {
    return float2(map(values.x, newMins.x, newMaxs.x), map(values.y, newMins.y, newMaxs.y));
}

float median(float a, float b, float c) {
    return max(min(a, b), min(max(a, b), c));
}

float4 GetGameObject(VertexOutput input) {
    float2 uv = map(input.BaseUV, input.UV.xy, input.UV.xy + input.UV.zw);
    float4 color = tex2Dgrad(GameAtlasSample, uv, ddx(uv), ddy(uv));
    
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
    
    float2 offsetX = {1.0 / 4.0 / 4096.0, 0};
    float2 offsetY = {0, 1.0 / 4.0 / 4096.0};
    float alpha = 0;
    float2 c;

    c = uv + offsetX - offsetY;
    alpha = max(alpha, tex2D(GameAtlasSample, c).a);
    c = uv + offsetX + offsetY;
    alpha = max(alpha, tex2D(GameAtlasSample, c).a);
    c = uv - offsetX - offsetY;
    alpha = max(alpha, tex2D(GameAtlasSample, c).a);
    c = uv - offsetX + offsetY;
    alpha = max(alpha, tex2D(GameAtlasSample, c).a);


    if (alpha > 0) {
        color = float4(0, 0, 0, 1);
        color.a *= input.Extra.w;
        return color;
    }
    
    color = input.Color;
    c = uv;
    
    float sum = 0.0;
    
    float px = 1.0 / 4.0 / 4096.0;
    float py = 1.0 / 4.0 / 4096.0;

    float x2 = px * 2;
    float x3 = px * 3;

    float y3 = py * -3.5;

    sum += tex2D(GameAtlasSample, c - float2(x3, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(0.0, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x3, y3)).a;

    y3 = py * -2.5;

    sum += tex2D(GameAtlasSample, c - float2(x3, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(0.0, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x3, y3)).a;

    y3 = py * -1.5;

    sum += tex2D(GameAtlasSample, c - float2(x3, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(0.0, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x3, y3)).a;

    y3 = py * -0.5;

    sum += tex2D(GameAtlasSample, c - float2(x3, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(0.0, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x3, y3)).a;

    y3 = py * 0.5;

    sum += tex2D(GameAtlasSample, c - float2(x3, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(0.0, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x3, y3)).a;

    y3 = py * 1.5;

    sum += tex2D(GameAtlasSample, c - float2(x3, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(0.0, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x3, y3)).a;

    y3 = py * 2.5;

    sum += tex2D(GameAtlasSample, c - float2(x3, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(0.0, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x3, y3)).a;
    
    y3 = py * 3.5;
    
    sum += tex2D(GameAtlasSample, c - float2(x3, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c - float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(0.0, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(px, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x2, y3)).a;
    sum += tex2D(GameAtlasSample, c + float2(x3, y3)).a;

    color.a = sum / 49.0;
    
    return color;
}

//todo bring over ui text improvements
float4 GetText(VertexOutput input) {
    float2 uv = map(input.BaseUV, input.UV.xy, input.UV.xy + input.UV.zw);
    float3 samp = tex2D(TextSample, uv).rgb;
    float pRange = PixelRange;
    float2 dim = TextTextureSize;
    
    float2 msdfUnit = pRange / dim;
   

    float sigDist = median(samp.r, samp.g, samp.b) - 0.5f;
    sigDist = sigDist * dot(msdfUnit, 0.5f / fwidth(uv));
    const float strokeThickness = 0.250f * 0.75f;
    float strokeDist = median(samp.r, samp.g, samp.b) - 0.25f * (1.0 + (pRange - 12) / pRange) - strokeThickness;
    strokeDist = -(abs(strokeDist) - strokeThickness);
    strokeDist = strokeDist * dot(msdfUnit, 0.5f / fwidth(uv));

    float opacity = clamp(sigDist + 0.5f, 0.0f, 1.0f);
    float strokeOpacity = clamp(strokeDist + 0.5f, 0.0f, 1.0f);
    return lerp(float4(0, 0, 0, 1), input.Color, opacity) * max(opacity, strokeOpacity);
}

PixelOut MainPixelShader(VertexOutput input) {
    PixelOut output;
    
    float id = input.Extra.x;
    
    if (id == TypeGameObject || id == TypeModel || id == TypeWall) { 
        output.Color = GetGameObject(input);
    } else if (id == TypeText) {
        output.Color = GetText(input);
    } else if (id == TypeBar) {
        output.Color = input.Color;
    } else {
        output.Color = float4(0, 0, 0, 0);
    }
    
    output.Depth = input.Extra.y;
    
    output.Color.a *= input.Extra.w;
    
    if(output.Color.a == 0) {
        discard;
    }
    
    return output;
}

technique GroundDraw {
	pass P0 {
	    AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
		VertexShader = compile VS_SHADERMODEL MainVertexShader();
		PixelShader = compile PS_SHADERMODEL MainPixelShader();
	}
};