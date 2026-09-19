#include <metal_stdlib>
using namespace metal;

fragment float4 fragment_main(
                    VertexOut in [[stage_in]],
                    constant ObjectData& object [[buffer(2)]],
                    texture2d<float> texture [[texture(0)]],
                    sampler sampler [[sampler(0)]]
                )
{
    float4 color = object.objColor;
    if (object.useTexture) {
        color = object.texColor * texture.sample(sampler, in.texCoord);
    }
    if (color.a == 0) discard_fragment();
    return color;
}