#version 450 core

uniform sampler2D GameTexture;

out vec4 FragColor;

in vec2 BaseUV;
in vec4 Color;
in float Depth;

void main() {
    gl_FragDepth = Depth;
    
    if (BaseUV.x < 0.1 || BaseUV.x > 0.9 || BaseUV.y < 0.1 || BaseUV.y > 0.9) {
        FragColor = vec4(0, 0, 0, 1);
    } else if (Color.w > -1) {
        FragColor = texture(GameTexture, Color.xy);
    } else {
        FragColor = vec4(Color.xyz, 1);
    }
}

