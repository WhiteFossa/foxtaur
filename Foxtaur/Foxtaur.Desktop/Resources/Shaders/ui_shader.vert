#version 330 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec2 vTexCoords;

out vec2 fTexCoords;

void main()
{
    gl_Position = vec4(2.0 * vPos.x - 1.0, 2.0 * vPos.y - 1.0, 0.0, 1.0);
    fTexCoords = vTexCoords;
}