#version 400 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec2 vTexCoords;

out vec2 fTexCoords;

void main()
{
    gl_Position = vec4(vPos, 1.0);

    fTexCoords = vTexCoords;
}