namespace odl3d;

internal static partial class DefaultShaders
{
    private const string OpenGLVertex = """

#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texCoord;
layout(location = 2) in vec3 normal;

layout(std140, binding = 0) uniform ObjectData
{
    mat4 model;
    vec4 texColor;
    vec4 objColor;
    uint useTexture;
    uint hasNormals;
    uint lit;
    float shininess;
    vec4 emissive;
    vec4 specular;
    mat4 normalMatrix;
} object;

layout(std140, binding = 1) uniform SceneData
{
    mat4 projection;
    mat4 view;
} scene;

out vec2 vTexCoord;
out vec3 vWorldPos;
out vec3 vWorldNormal;

void main()
{
    mat4 mvp = scene.projection * scene.view * object.model;
    gl_Position = mvp * vec4(position, 1.0);
    gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
    vTexCoord = texCoord;
    vWorldPos = (object.model * vec4(position, 1.0)).xyz;
    vWorldNormal = (object.normalMatrix * vec4(normal, 0.0)).xyz;
}

""";
}
