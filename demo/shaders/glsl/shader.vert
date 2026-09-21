#version 330 core

layout(location = 0) in vec3 position;   // attribute(0)
layout(location = 1) in vec2 texCoord;   // attribute(1)

out vec2 vTexCoord;

layout(std140, binding = 0) uniform ObjectData
{
    mat4 model;
    vec4 texColor;
    vec4 objColor;
    uint useTexture;
    uint hasNormals;
} object;

layout(std140, binding = 1) uniform SceneData
{
    mat4 view;
    mat4 projection;
} scene;

void main()
{
    mat4 mvp = scene.projection * scene.view * object.model;
    gl_Position = mvp * vec4(position, 1.0);

    // System.Numerics/Metal clip depth is 0..w, GL's is -w..w
    gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;

    vTexCoord = texCoord;
}