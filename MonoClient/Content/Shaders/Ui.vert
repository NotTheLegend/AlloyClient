#version 330

uniform mat4 ViewMatrix;

layout (location = 0) in vec2 Position;
layout (location = 1) in uint Color;
layout (location = 2) in uint Override;
layout (location = 3) in vec2 Info;
layout (location = 4) in vec2 UVCoords;
layout (location = 5) in vec4 Scissor;
layout (location = 6) in vec4 Extra1;
layout (location = 7) in vec4 Extra2;
layout (location = 8) in vec4 ColorTransform;

out VS_OUT {
    vec4 Position1;
    flat uint Color;
    flat uint Override;
    vec2 Info;
    vec2 UVCoords;
    vec4 Scissor;
    vec4 Extra1;
    vec4 Extra2;
    vec4 ColorTransform;
} output;

void main() {
    gl_Position = vec4(Position, 0, 1) * ViewMatrix;
    output.Position1 = gl_Position;
    output.Color = Color;
    output.Override = Override;
    output.Info = Info;
    output.UVCoords = UVCoords;
    output.Scissor.xy = (vec4(Scissor.x, Scissor.y, 0, 1) * ViewMatrix).xy;
    output.Scissor.zw = (vec4(Scissor.z, Scissor.w, 0, 1) * ViewMatrix).xy;
    output.Extra1 = Extra1;
    output.Extra2 = Extra2;
    output.ColorTransform = ColorTransform;
}