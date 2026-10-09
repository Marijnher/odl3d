namespace odl3d;

internal static partial class DefaultShaders
{
    // __MAX_LIGHTS__ is replaced with the number of lights one uniform block can hold on the current device.
    private const string OpenGLFragment = """

#version 330 core

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

struct Light
{
    vec4 positionKind;
    vec4 colorRange;
    vec4 directionA;
    vec4 tangentB;
};

layout(std140, binding = 2) uniform LightingData
{
    vec4 cameraPosition;
    vec4 ambient;
    uint lightCount;
} lighting;

layout(std140, binding = 3) uniform LightList
{
    Light lights[__MAX_LIGHTS__];
} lightList;

uniform sampler2D tex;

in vec2 vTexCoord;
in vec3 vWorldPos;
in vec3 vWorldNormal;
out vec4 fragColor;

// Smoothly fades a light to zero at its range; a range of zero means no cutoff.
float rangeWindow(float dist, float range)
{
    if (range <= 0.0) return 1.0;
    float ratio = clamp(dist / range, 0.0, 1.0);
    float f = 1.0 - ratio * ratio * ratio * ratio;
    return f * f;
}

void main()
{
    vec4 color = object.objColor;
    if (object.useTexture != 0u)
        color = object.texColor * texture(tex, vTexCoord);

    if (color.a == 0.0) discard;

    vec3 rgb = color.rgb;
    if (object.lit != 0u)
    {
        vec3 n = normalize(vWorldNormal);
        vec3 v = normalize(lighting.cameraPosition.xyz - vWorldPos);
        // Light both faces of a surface so flat geometry seen from behind is not black.
        bool backFace = dot(n, v) < 0.0;
        if (backFace) n = -n;

        vec3 diffuse = lighting.ambient.rgb;
        vec3 spec = vec3(0.0);
        for (uint i = 0u; i < lighting.lightCount; i++)
        {
            Light light = lightList.lights[i];
            int code = int(light.positionKind.w + 0.5);
            int kind = code & 7;
            float range = light.colorRange.w;
            vec3 l;
            float atten;

            if (kind == 1)
            {
                // Directional
                l = -light.directionA.xyz;
                atten = 1.0;
            }
            else if (kind >= 3)
            {
                // Area: light from the point of the surface closest to the shaded point.
                vec3 axisN = light.directionA.xyz;
                vec3 axisR = light.tangentB.xyz;
                vec3 axisU = cross(axisN, axisR);
                vec2 halfSize = vec2(light.directionA.w, light.tangentB.w);
                vec3 rel = vWorldPos - light.positionKind.xyz;
                vec2 local = vec2(dot(rel, axisR), dot(rel, axisU));
                if (kind == 3)
                {
                    local = clamp(local, -halfSize, halfSize);
                }
                else
                {
                    vec2 q = local / max(halfSize, vec2(1e-5));
                    float len = length(q);
                    if (len > 1.0) q /= len;
                    local = q * halfSize;
                }
                vec3 closest = light.positionKind.xyz + axisR * local.x + axisU * local.y;
                vec3 toLight = closest - vWorldPos;
                float dist = length(toLight);
                l = toLight / max(dist, 1e-5);
                float facing = dot(axisN, -l);
                facing = (code & 8) != 0 ? abs(facing) : max(facing, 0.0);
                atten = facing * rangeWindow(dist, range) / (1.0 + dist * dist);
            }
            else
            {
                // Point and spot
                vec3 toLight = light.positionKind.xyz - vWorldPos;
                float dist = length(toLight);
                l = toLight / max(dist, 1e-5);
                atten = rangeWindow(dist, range) / (1.0 + dist * dist);
                if (kind == 2)
                    atten *= smoothstep(light.directionA.w, light.tangentB.w, dot(-l, light.directionA.xyz));
            }

            vec3 radiance = light.colorRange.rgb * atten;
            // Single-sided geometry seen from behind (terrace undersides, planes) takes light from either side instead of going black.
            float ndl = backFace ? abs(dot(n, l)) : max(dot(n, l), 0.0);
            diffuse += radiance * ndl;
            if (ndl > 0.0)
            {
                vec3 h = normalize(l + v);
                spec += radiance * pow(max(dot(n, h), 0.0), object.shininess);
            }
        }
        rgb = rgb * diffuse + object.specular.rgb * spec + object.emissive.rgb;
    }

    fragColor = vec4(rgb, color.a);
}

""";
}
