#version 330 core

layout(std140, binding = 0) uniform ObjectData
{
    mat4 model;
    vec4 texColor;
    vec4 objColor;
    uint useTexture;
    uint hasNormals;
} object;

uniform sampler2D tex;

in vec2 vTexCoord;
out vec4 fragColor;

void main()
{
    vec4 color = object.objColor;
    if (object.useTexture != 0u)
        color = object.texColor * texture(tex, vTexCoord);

    if (color.a == 0.0) discard;
    fragColor = color;
}