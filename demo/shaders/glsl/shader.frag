#version 330 core

in vec2 vTexCoord;
in vec3 vNormal;

out vec4 FragColor;

uniform sampler2D uTexture;
uniform bool uUseTexture;
uniform bool uLit;
uniform vec4 uColor;
uniform vec4 texColor;

void main()
{
    vec4 color = uUseTexture ? texture(uTexture, vTexCoord) * texColor : uColor;

    // Discard fully transparent fragments
    if (color.a == 0)
        discard;

    // Meshes with normals (e.g. extruded 3D text) get simple two-sided directional lighting.
    if (uLit)
    {
        vec3 normal = normalize(vNormal);
        vec3 lightDirection = normalize(vec3(-0.35, 0.6, 1.0));
        float diffuse = max(abs(dot(normal, lightDirection)), 0.0);
        color.rgb *= 0.35 + 0.65 * diffuse;
    }

    FragColor = color;
}