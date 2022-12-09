#version 330 core

// Texture coordinates
in vec2 fTexCoords;

// Texture
uniform sampler2D ourTexture;

// Resulting color
out vec4 FragColor;

void main()
{
    FragColor = texture(ourTexture, fTexCoords);
}