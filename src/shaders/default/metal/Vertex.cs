namespace odl3d;

internal static partial class DefaultShaders
{
    private const string MetalVertex = """

#include <metal_stdlib>
using namespace metal;

struct VertexIn
{
    float3 position [[attribute(0)]];
    float2 texCoord [[attribute(1)]];
    float3 normal [[attribute(2)]];
};

struct SceneData
{
    float4x4 projection;
    float4x4 view;
};

struct ObjectData
{
    float4x4 model;
    float4 texColor;
    float4 objColor;
    uint useTexture;
    uint hasNormals;
    uint lit;
    float shininess;
    float4 emissive;
    float4 specular;
    float4x4 normalMatrix;
};

struct VertexOut
{
    float4 position [[position]];
    float2 texCoord;
    float3 worldPosition;
    float3 worldNormal;
};

vertex VertexOut vertex_main(
    VertexIn in [[stage_in]],
    constant ObjectData& object [[buffer(0)]],
    constant SceneData& scene [[buffer(1)]]
)
{
    float4x4 mvp = scene.projection * scene.view * object.model;
    VertexOut out;
    out.position = mvp * float4(in.position, 1.0);
    out.texCoord = in.texCoord;
    out.worldPosition = (object.model * float4(in.position, 1.0)).xyz;
    out.worldNormal = (object.normalMatrix * float4(in.normal, 0.0)).xyz;
    return out;
}

""";
}
