#version 330

out vec4 FragColor;

in VS {
    vec2 BaseUV;
    flat uint Color;
} input;

// ABGR
vec3 unpackColor(uint color) {
    return vec3(
    float(color & 0x0000FFu) / 255.0,
    float((color & 0x00FF00u) >> 8u) / 255.0,
    float((color & 0xFF0000u) >> 16u) / 255.0
    );
}

void main() {
    float dx = 0.5 - input.BaseUV.x, dy = 0.5 - input.BaseUV.y;
    float dist = dx * dx + dy * dy;
    float distFromCenter = 0.25 - dist;
    FragColor = vec4(unpackColor(input.Color), distFromCenter * 1.5);
}

