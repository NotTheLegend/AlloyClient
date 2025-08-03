#version 330

uniform mat4x4 WorldMatrix;
uniform mat4x4 ViewMatrix;
uniform mat4x4 ProjMatrix;

layout(location = 0) in vec3 BasePosition;
layout(location = 1) in vec2 BaseUV;
layout(location = 2) in vec3 Position;
layout(location = 3) in vec2 Scale;
layout(location = 4) in uint Color;

out VS {
    vec2 BaseUV;
    flat uint Color;
} output;

void main() {
    vec3 pos = BasePosition;
    pos.xy *= Scale;
    pos.xyz += Position.xyz;
    
    gl_Position = vec4(pos, 1.0) * WorldMatrix * ViewMatrix * ProjMatrix;
    output.BaseUV = BaseUV;
    output.Color = Color;
}