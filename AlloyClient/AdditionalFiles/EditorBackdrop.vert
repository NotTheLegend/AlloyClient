#version 460 core

uniform vec2 UvOrigin;
uniform vec2 UvSize;
uniform vec2 UvScale;

const vec2 positions[6] = vec2[6](
    vec2(-1.0, -1.0), vec2(1.0, -1.0), vec2(-1.0, 1.0),
    vec2(-1.0, 1.0), vec2(1.0, -1.0), vec2(1.0, 1.0)
);

const vec2 texCoords[6] = vec2[6](
    vec2(0.0, 1.0), vec2(1.0, 1.0), vec2(0.0, 0.0),
    vec2(0.0, 0.0), vec2(1.0, 1.0), vec2(1.0, 0.0)
);

out vec2 TextureCoordinate;

void main() {
    gl_Position = vec4(positions[gl_VertexID], 0.999, 1.0);
    vec2 centeredUv = vec2(0.5) + (texCoords[gl_VertexID] - vec2(0.5)) * UvScale;
    TextureCoordinate = UvOrigin + centeredUv * UvSize;
}
