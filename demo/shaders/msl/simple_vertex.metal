#include <metal_stdlib>
using namespace metal;

struct VertexIn
{
    packed_float3 position;
    packed_float2 texCoord;
};

struct VertexOut
{
    float4 position [[position]];
    float2 texCoord;
};

vertex VertexOut vertex_main(
                    uint vertexID [[vertex_id]],
                    device const VertexIn* vertices [[buffer(0)]],
                    constant float4x4& camera [[buffer(1)]]
                )
{
    VertexOut out;
    out.position = camera * float4(vertices[vertexID].position, 1.0);
    out.texCoord = vertices[vertexID].texCoord;
    return out;
}