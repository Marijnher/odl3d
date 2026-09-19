#include <metal_stdlib>
using namespace metal;

fragment float4 fragment_main(
                    VertexOut in [[stage_in]],
                    constant ObjectData& object [[buffer(2)]],
                    texture2d<float> texture [[texture(0)]],
                    sampler sampler [[sampler(0)]]
                )
{
    float4 color = float4(0, 0, 1.0, 1.0);
    if (object.useTexture) {
        color = object.texColor * texture.sample(sampler, in.texCoord);
    }
    else {
        color = object.objColor;
    }
    return color;
}