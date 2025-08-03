#version 330

uniform mat4x4 WorldMatrix;
uniform mat4x4 ViewMatrix;
uniform mat4x4 ProjMatrix;
uniform mat4x4 BillMatrix;

layout(location = 0) in vec3 BasePosition;
layout(location = 1) in vec2 BaseUV;
layout(location = 2) in vec3 Position;
layout(location = 3) in vec4 Color;

out VS {
    vec2 BaseUV;
    vec4 Color;
    float Depth;
} output;

void main() {
    mat4x4 billboard = BillMatrix;
    billboard[0][3] = Position.x;
    billboard[1][3] = Position.y;
    billboard[2][3] = Position.z;

    gl_Position = vec4(BasePosition, 1.0) * billboard * WorldMatrix * ViewMatrix * ProjMatrix;
    output.BaseUV = BaseUV;
    output.Color = Color;
    vec4 depth = vec4(Position.xy, 0, 1) * WorldMatrix * ViewMatrix * ProjMatrix;
    output.Depth = 0.5f + 0.4f * depth.y;
}