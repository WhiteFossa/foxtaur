#version 400 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec2 vTexCoords;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

varying vec2 v_rgbNW;
varying vec2 v_rgbNE;
varying vec2 v_rgbSW;
varying vec2 v_rgbSE;
varying vec2 v_rgbM;

uniform vec2 resolution;

out vec2 fTexCoords;

void texcoords(vec2 fragCoord, vec2 resolution,
out vec2 v_rgbNW, out vec2 v_rgbNE,
out vec2 v_rgbSW, out vec2 v_rgbSE,
out vec2 v_rgbM) {
    vec2 inverseVP = 1.0 / resolution.xy;
    v_rgbNW = (fragCoord + vec2(-1.0, -1.0)) * inverseVP;
    v_rgbNE = (fragCoord + vec2(1.0, -1.0)) * inverseVP;
    v_rgbSW = (fragCoord + vec2(-1.0, 1.0)) * inverseVP;
    v_rgbSE = (fragCoord + vec2(1.0, 1.0)) * inverseVP;
    v_rgbM = vec2(fragCoord * inverseVP);
}

void main()
{
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
    
    vec2 fragCoord = vTexCoords * resolution;
    texcoords(fragCoord, resolution, v_rgbNW, v_rgbNE, v_rgbSW, v_rgbSE, v_rgbM);
    
    fTexCoords = vTexCoords;
}