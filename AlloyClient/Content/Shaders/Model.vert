#version 460 core

uniform mat4 FullMatrix;

layout (location = 0) in vec3 Position;
layout (location = 1) in vec2 BaseUV;

layout (location = 2) in vec3 iPosition;
layout (location = 3) in vec4 iUV;
layout (location = 4) in vec3 iExtra;

out MODEL_OUT {
    vec2 BaseUV;
    vec4 UV;
    vec3 Extra;
    float Zed;
} output1;

void main() {
    float s = sin(iExtra.x);
    float c = cos(iExtra.x);

    vec4 pos = vec4(Position.xy * mat2(c, -s, s, c), Position.z, 1);
    pos = vec4((pos.xy - 0.5) * 1.005 + 0.5, pos.zw);
    pos.xyz += iPosition;
    
    output1.BaseUV = BaseUV;
    output1.UV = iUV;
    output1.Extra = iExtra;
    output1.Zed = pos.z;

    pos = pos * FullMatrix;
    pos.z = iExtra.y;
    
    gl_Position = pos;
}