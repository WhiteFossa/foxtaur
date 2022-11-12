#version 400 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec4 vColor;

out vec4 fColor;

void main()
{
    gl_Position = vec4(vPos, 1.0);
    
    fColor = vColor;
}