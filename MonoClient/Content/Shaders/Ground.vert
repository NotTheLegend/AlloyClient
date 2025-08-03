#version 330

uniform mat4 WorldMatrix;
uniform mat4 ViewMatrix;
uniform mat4 ProjMatrix;
uniform float GameTime;

layout (location = 0) in vec3 Position;
layout (location = 1) in vec2 BaseUV;
layout (location = 2) in vec4 iPosition;
layout (location = 3) in vec4 UV;
layout (location = 4) in vec4 Animate;
layout (location = 5) in vec4 BlendLeftRight;
layout (location = 6) in vec4 BlendTopBottom;
layout (location = 7) in vec4 CornerBottom;
layout (location = 8) in vec4 CornerTop;

out VS {
    vec2 BaseUV;
    vec4 UV;
    vec4 Animate;
    vec4 BlendLeftRight;
    vec4 BlendTopBottom;
    vec4 CornerBottom;
    vec4 CornerTop;
    vec2 CoreUV;
} output;

void main() {
    vec4 inputPosition = vec4(Position, 1);
    inputPosition.xy += iPosition.xy;
    gl_Position = inputPosition * WorldMatrix * ViewMatrix * ProjMatrix;

    output.BaseUV = BaseUV;
    output.CoreUV.x = BaseUV.x + iPosition.z + sin(GameTime * Animate.x) + GameTime * Animate.z;
    output.CoreUV.y = BaseUV.y + iPosition.w + sin(GameTime * Animate.y) + GameTime * Animate.w;

    output.UV = UV;
    output.Animate = Animate;
    output.BlendLeftRight = BlendLeftRight;
    output.BlendTopBottom = BlendTopBottom;
    output.CornerBottom = CornerBottom;
    output.CornerTop = CornerTop;
}