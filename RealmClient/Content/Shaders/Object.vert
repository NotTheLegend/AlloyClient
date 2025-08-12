#version 330

uniform mat4 WorldMatrix;
uniform mat4 ViewMatrix;
uniform mat4 ProjMatrix;
uniform mat4 BillMatrix;

layout (location = 0) in vec3 Position;
layout (location = 1) in vec2 BaseUV;

layout (location = 2) in vec3 iPosition;
layout (location = 3) in vec4 iUV;
layout (location = 4) in vec4 iScale;
layout (location = 5) in vec4 iRotation;
layout (location = 6) in vec4 iExtra;
layout (location = 7) in vec4 iColor;
//layout (location = 8) in vec4 iMask1;
//layout (location = 9) in vec4 iMask2;

out VS_OUT {
    vec2 BaseUV;
    vec4 UV;
    vec4 Extra;
    vec4 Color;
    vec4 Mask1;
    vec4 Mask2;
    float Depth;
} output;

const float TypeGameObject = 0.0;
const float TypeModel = 1.0;
const float TypeWall = 2.0;
const float TypeText = 3.0;
const float TypeBar = 4.0;

vec4 GetPosition(vec3 position, vec3 dataPosition, vec4 dataScale, vec4 rot, vec4 dataExtra) {
    float id = dataExtra.x;
    if (id == TypeGameObject || id == TypeText || id == TypeBar) {
        vec4 pos = vec4(0);
        
        position.xy *= dataScale.xy;
        
        mat4 rotate = mat4(
            rot.y * rot.z, rot.x * rot.z, 0, dataScale.z * rot.z * -rot.w,
            -rot.x * rot.z, rot.y * rot.z, 0, dataScale.w * rot.z,
            0, 0, 1, 0,
            0, 0, 0, 1
        );
        
        pos = vec4(position, 1) * rotate * BillMatrix;
        pos.xyz += dataPosition;

        return pos;
    } else if (id == TypeModel){
        float s = sin(rot.x);
        float c = cos(rot.x);
        
        mat2 rot = mat2(c, -s, s, c);
        
        vec4 pos = vec4(position.xy * rot, position.z, 1);
        pos.xyz += dataPosition;
        return pos;
    } else if (id == TypeWall) {
        position.xyz += dataPosition.xyz;
        position.xy -= 0.5;

        return vec4(position, 1);
    } else {
        position.xyz += dataPosition.xyz;
        return vec4(position, 1);
    }
}

vec2 GetUV(vec2 uv, vec4 dataExtra) {
    float id = dataExtra.x;
    if (id == TypeGameObject) {
        uv.x = 0.5 + (0.5 - uv.x) * dataExtra.w;
    }

    return uv;
}

void main() {
    gl_Position = GetPosition(Position, iPosition, iScale, iRotation, iExtra) * WorldMatrix * ViewMatrix * ProjMatrix;
    output.BaseUV = GetUV(BaseUV, iExtra);
    output.UV = iUV;
    output.Extra = iExtra;
    output.Color = iColor;
    //output.Mask1 = iMask1;
    //output.Mask2 = iMask2;
    output.Depth = iExtra.y;
}