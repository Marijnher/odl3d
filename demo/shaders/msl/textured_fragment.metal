#include <metal_stdlib>
using namespace metal;

fragment float4 fragment_main(
                    VertexOut in [[stage_in]],
                    texture2d<float> texture [[texture(0)]],
                    sampler sampler [[sampler(0)]]
                )
{
    float4 color = texture.sample(sampler, in.texCoord);
    return color;
}