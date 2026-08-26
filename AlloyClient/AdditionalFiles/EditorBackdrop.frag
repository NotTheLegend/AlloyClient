#version 460 core

uniform sampler2D BackdropTexture;
in vec2 TextureCoordinate;
out vec4 FragColor;

void main() {
    FragColor = texture(BackdropTexture, TextureCoordinate);
}
