namespace odl3d;

internal static class DefaultShaders
{
    public static string Vertex(RenderTarget target) => target switch
    {
        RenderTarget.OpenGL => OpenGLVertex,
        RenderTarget.Metal => MetalVertex,
        _ => throw new RenderException($"Unsupported render target: {target}.")
    };

    public static string Fragment(RenderTarget target) => target switch
    {
        RenderTarget.OpenGL => OpenGLFragment,
        RenderTarget.Metal => MetalFragment,
        _ => throw new RenderException($"Unsupported render target: {target}.")
    };

    private const string OpenGLVertex = """
        #version 330 core

        layout(location = 0) in vec3 position;
        layout(location = 1) in vec2 texCoord;
        layout(location = 2) in vec3 normal;

        layout(std140, binding = 0) uniform ObjectData
        {
            mat4 model;
            vec4 texColor;
            vec4 objColor;
            uint useTexture;
            uint hasNormals;
        } object;

        layout(std140, binding = 1) uniform SceneData
        {
            mat4 projection;
            mat4 view;
        } scene;

        out vec2 vTexCoord;

        void main()
        {
            mat4 mvp = scene.projection * scene.view * object.model;
            gl_Position = mvp * vec4(position, 1.0);
            gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
            vTexCoord = texCoord;
        }
        """;

    private const string OpenGLFragment = """
        #version 330 core

        layout(std140, binding = 0) uniform ObjectData
        {
            mat4 model;
            vec4 texColor;
            vec4 objColor;
            uint useTexture;
            uint hasNormals;
        } object;

        layout(std140, binding = 1) uniform SceneData
        {
            mat4 projection;
            mat4 view;
        } scene;

        uniform sampler2D tex;

        in vec2 vTexCoord;
        out vec4 fragColor;

        void main()
        {
            vec4 color = object.objColor;
            if (object.useTexture != 0u)
                color = object.texColor * texture(tex, vTexCoord);

            if (color.a == 0.0) discard;
            fragColor = color;
        }
        """;

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
        };

        struct VertexOut
        {
            float4 position [[position]];
            float2 texCoord;
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
            return out;
        }
        """;

    private const string MetalFragment = """
        #include <metal_stdlib>
        using namespace metal;

        fragment float4 fragment_main(
            VertexOut in [[stage_in]],
            constant ObjectData& object [[buffer(0)]],
            texture2d<float> texture [[texture(0)]],
            sampler sampler [[sampler(0)]]
        )
        {
            float4 color = object.objColor;
            if (object.useTexture)
                color = object.texColor * texture.sample(sampler, in.texCoord);
            if (color.a == 0) discard_fragment();
            return color;
        }
        """;
}