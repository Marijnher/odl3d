#include <metal_stdlib>
using namespace metal;

fragment float4 fragment_main(
                    VertexOut in [[stage_in]]
                )
{
    float4 color = float4(in.texCoord, 1.0, 1.0);
    return color;
}