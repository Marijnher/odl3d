#include <metal_stdlib>
using namespace metal;

struct VertexIn
{
    float3 position [[attribute(0)]];
    float2 texCoord [[attribute(1)]];
};

struct VertexOut
{
    float4 position [[position]];
    float2 texCoord;
};

vertex VertexOut vertex_main(
                    uint vertexID [[vertex_id]],
                    VertexIn in [[stage_in]],
                    constant float4x4& camera [[buffer(1)]]
                )
{
    VertexOut out;
    out.position = camera * float4(in.position, 1.0);
    out.texCoord = in.texCoord;
    return out;
}