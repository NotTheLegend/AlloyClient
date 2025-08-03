#version 330

uniform sampler2D atlas;

out vec4 FragColor;

in VS {
    vec2 BaseUV;
    vec4 Color;
    float Depth;
} input;

void main() {
    gl_FragDepth = input.Depth;
    
    if (input.BaseUV.x < 0.1 || input.BaseUV.x > 0.9 || input.BaseUV.y < 0.1 || input.BaseUV.y > 0.9) {
        FragColor = vec4(0, 0, 0, 1);
    } else if (input.Color.w > -1) {
        FragColor = texture(atlas, input.Color.xy);
    } else {
        FragColor = vec4(input.Color.xyz, 1);
    }
}

