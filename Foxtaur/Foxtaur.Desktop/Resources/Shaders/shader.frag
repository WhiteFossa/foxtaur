#version 400 core

in vec2 fTexCoords;

uniform sampler2D ourTexture;

out vec4 FragColor;

void main()
{
    FragColor = texture(ourTexture, fTexCoords);
}