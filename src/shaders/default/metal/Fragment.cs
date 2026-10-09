namespace odl3d;

internal static partial class DefaultShaders
{
    private const string MetalFragment = """

struct Light
{
    float4 positionKind;
    float4 colorRange;
    float4 directionA;
    float4 tangentB;
};

struct LightingData
{
    float4 cameraPosition;
    float4 ambient;
    uint lightCount;
};

// Smoothly fades a light to zero at its range; a range of zero means no cutoff.
static float rangeWindow(float dist, float range)
{
    if (range <= 0.0) return 1.0;
    float ratio = clamp(dist / range, 0.0, 1.0);
    float f = 1.0 - ratio * ratio * ratio * ratio;
    return f * f;
}

fragment float4 fragment_main(
    VertexOut in [[stage_in]],
    constant ObjectData& object [[buffer(0)]],
    constant LightingData& lighting [[buffer(2)]],
    constant Light* lights [[buffer(3)]],
    texture2d<float> texture [[texture(0)]],
    sampler sampler [[sampler(0)]]
)
{
    float4 color = object.objColor;
    if (object.useTexture)
        color = object.texColor * texture.sample(sampler, in.texCoord);
    if (color.a == 0) discard_fragment();

    float3 rgb = color.rgb;
    if (object.lit != 0)
    {
        float3 n = normalize(in.worldNormal);
        float3 v = normalize(lighting.cameraPosition.xyz - in.worldPosition);
        // Light both faces of a surface so flat geometry seen from behind is not black.
        bool backFace = dot(n, v) < 0.0;
        if (backFace) n = -n;

        float3 diffuse = lighting.ambient.rgb;
        float3 spec = float3(0.0);
        for (uint i = 0; i < lighting.lightCount; i++)
        {
            Light light = lights[i];
            int code = int(light.positionKind.w + 0.5);
            int kind = code & 7;
            float range = light.colorRange.w;
            float3 l;
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
                float3 axisN = light.directionA.xyz;
                float3 axisR = light.tangentB.xyz;
                float3 axisU = cross(axisN, axisR);
                float2 halfSize = float2(light.directionA.w, light.tangentB.w);
                float3 rel = in.worldPosition - light.positionKind.xyz;
                float2 local = float2(dot(rel, axisR), dot(rel, axisU));
                if (kind == 3)
                {
                    local = clamp(local, -halfSize, halfSize);
                }
                else
                {
                    float2 q = local / max(halfSize, float2(1e-5));
                    float len = length(q);
                    if (len > 1.0) q /= len;
                    local = q * halfSize;
                }
                float3 closest = light.positionKind.xyz + axisR * local.x + axisU * local.y;
                float3 toLight = closest - in.worldPosition;
                float dist = length(toLight);
                l = toLight / max(dist, 1e-5);
                float facing = dot(axisN, -l);
                facing = (code & 8) != 0 ? abs(facing) : max(facing, 0.0);
                atten = facing * rangeWindow(dist, range) / (1.0 + dist * dist);
            }
            else
            {
                // Point and spot
                float3 toLight = light.positionKind.xyz - in.worldPosition;
                float dist = length(toLight);
                l = toLight / max(dist, 1e-5);
                atten = rangeWindow(dist, range) / (1.0 + dist * dist);
                if (kind == 2)
                    atten *= smoothstep(light.directionA.w, light.tangentB.w, dot(-l, light.directionA.xyz));
            }

            float3 radiance = light.colorRange.rgb * atten;
            // Single-sided geometry seen from behind (terrace undersides, planes) takes light from either side instead of going black.
            float ndl = backFace ? abs(dot(n, l)) : max(dot(n, l), 0.0);
            diffuse += radiance * ndl;
            if (ndl > 0.0)
            {
                float3 h = normalize(l + v);
                spec += radiance * pow(max(dot(n, h), 0.0), object.shininess);
            }
        }
        rgb = rgb * diffuse + object.specular.rgb * spec + object.emissive.rgb;
    }

    return float4(rgb, color.a);
}

""";
}
