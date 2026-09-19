#include <metal_stdlib>
using namespace metal;

struct VertexIn
{
    float3 position [[attribute(0)]];
    float2 texCoord [[attribute(1)]];
};

struct SceneData
{
    float4x4 view;
    float4x4 projection;
};

struct ObjectData
{
    float4x4 model;
};

struct VertexOut
{
    float4 position [[position]];
    float2 texCoord;
};

vertex VertexOut vertex_main(
                    VertexIn in [[stage_in]],
                    constant SceneData& scene [[buffer(1)]],
                    constant ObjectData& object [[buffer(2)]]
                )
{
    float4x4 mvp = scene.projection * scene.view * object.model;
    VertexOut out;
    out.position = mvp * float4(in.position, 1.0);
    out.texCoord = in.texCoord;
    return out;
}