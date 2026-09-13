using System;
using System.Numerics;

namespace odl3d;

public interface IRenderer : IDisposable
{
    /// <summary>
    /// Initializes the renderer, setting up any necessary graphics context, resources, and state for rendering. This method should be called before any rendering operations are performed, and it prepares the renderer to handle drawing commands, buffer management, and shader compilation.
    /// </summary>
    public void Initialize();

    /// <summary>
    /// Sets the viewport for rendering, defining the rectangular area of the window where rendering will occur. The viewport is specified by its lower-left corner (x, y) and its width and height. This method configures the renderer to map normalized device coordinates to the specified viewport area, allowing for proper rendering of 3D objects within the defined region of the window.
    /// </summary>
    /// <param name="x">The X coordinate of the viewport's lower-left corner.</param>
    /// <param name="y">The Y coordinate of the viewport's lower-left corner.</param>
    /// <param name="width">The width of the viewport.</param>
    /// <param name="height">The height of the viewport.</param>
    public void SetViewport(int x, int y, int width, int height);

    /// <summary>
    /// Enables or disables depth testing in the renderer. When depth testing is enabled, the renderer compares the depth values of incoming fragments against the existing depth buffer to determine whether a fragment should be drawn or discarded. This is essential for correctly rendering 3D scenes with overlapping objects, ensuring that closer objects obscure those that are farther away. Disabling depth testing can be useful for rendering transparent objects or 2D overlays.
    /// </summary>
    /// <param name="enable">True to enable depth testing, false to disable it.</param>
    public void SetEnableDepthTest(bool enable);

    /// <summary>
    /// Enables or disables writing to the depth buffer in the renderer. When depth writing is enabled, the depth values of incoming fragments are written to the depth buffer, allowing for proper occlusion of objects based on their distance from the camera. Disabling depth writing can be useful for rendering transparent objects or special effects where depth information should not be updated.
    /// </summary>
    /// <param name="enable">True to enable depth writing, false to disable it.</param>
    public void SetDepthMask(bool enable);

    /// <summary>
    /// Enables or disables alpha blending in the renderer. When alpha blending is enabled, the renderer combines the color of incoming fragments with the color already in the framebuffer based on their alpha values, allowing for transparency effects. Disabling alpha blending results in opaque rendering, where incoming fragments completely overwrite existing colors. This method is essential for rendering scenes with transparent objects, such as glass or water.
    /// </summary>
    /// <param name="enable"></param>
    public void SetAlphaBlending(bool enable);

    /// <summary>
    /// Enables or disables wireframe rendering mode in the renderer. When wireframe mode is enabled, the renderer draws only the edges of polygons, allowing for a clear view of the underlying geometry and structure of 3D models. Disabling wireframe mode results in solid rendering, where polygons are filled with their assigned colors or textures. This method is useful for debugging, visualizing mesh topology, and creating stylistic effects in 3D scenes.
    /// </summary>
    /// <param name="enable">True to enable wireframe mode, false to disable it.</param>
    public void SetWireFrame(bool enable);

    #region Buffer Methods
    /// <summary>
    /// Creates a new vertex array object (VAO) in the renderer, which encapsulates the state of vertex attributes and buffer bindings for rendering. The VAO is used to efficiently manage and switch between different vertex attribute configurations, allowing for optimized rendering of meshes with varying vertex formats. This method returns a handle to the newly created VAO, which can be used in subsequent rendering operations.
    /// </summary>
    /// <returns>A handle to the newly created vertex array object (VAO).</returns>
    public uint CreateVertexArray();
    /// <summary>
    /// Deletes the specified vertex array object (VAO) in the renderer, releasing any associated resources and state. The VAO is no longer valid after this operation, and any attempts to bind or use it will result in undefined behavior. This method is used to manage the lifecycle of VAOs, ensuring that resources are properly cleaned up when they are no longer needed.
    /// </summary>
    /// <param name="vao">The handle to the vertex array object (VAO) to delete.</param>
    public void DeleteVertexArray(uint vao);
    /// <summary>
    /// Binds the specified vertex array object (VAO) in the renderer, making it the current VAO for subsequent rendering operations. The VAO encapsulates the state of vertex attributes and buffer bindings, allowing for efficient switching between different vertex configurations. Binding a VAO ensures that the correct vertex attribute setup is used when drawing meshes, enabling proper rendering of 3D objects with varying vertex formats.
    /// </summary>
    /// <param name="vao">The handle to the vertex array object (VAO) to bind.</param>
    public void BindVertexArray(uint vao);
    /// <summary>
    /// Creates a new buffer object in the renderer, which can be used to store vertex data, index data, or other types of data for rendering. The buffer object is allocated in GPU memory and can be bound to different targets (e.g., array buffer, element array buffer) for use in rendering operations. This method returns a handle to the newly created buffer object, which can be used in subsequent rendering operations.
    /// </summary>
    /// <returns>A handle to the newly created buffer object.</returns>
    public uint CreateBuffer();
    /// <summary>
    /// Deletes the specified buffer object in the renderer, releasing any associated resources and memory. The buffer object is no longer valid after this operation, and any attempts to bind or use it will result in undefined behavior. This method is used to manage the lifecycle of buffer objects, ensuring that GPU resources are properly cleaned up when they are no longer needed.
    /// </summary>
    /// <param name="buffer">The handle to the buffer object to delete.</param>
    public void DeleteBuffer(uint buffer);
    /// <summary>
    /// Binds the specified buffer object to the given target in the renderer, making it the current buffer for subsequent operations. The target specifies the type of data the buffer will hold (e.g., vertex data, index data), and binding the buffer allows for data uploads, attribute configuration, and rendering operations. This method is essential for managing buffer state and ensuring that the correct buffer is used during rendering.
    /// </summary>
    /// <param name="target">The target to bind the buffer to.</param>
    /// <param name="buffer">The handle to the buffer object to bind.</param>
    public void BindBuffer(BufferTarget target, uint buffer);
    /// <summary>
    /// Uploads data to the specified buffer object in the renderer, replacing its current contents with the provided data. The target specifies the type of data the buffer holds (e.g., vertex data, index data), and the hint indicates how the data will be used (e.g., static draw, dynamic draw). This method is used to transfer vertex or index data from CPU memory to GPU memory, enabling efficient rendering of 3D objects with varying geometry and attributes.
    /// </summary>
    /// <param name="target">The target to upload data to.</param>
    /// <param name="buffer">The handle to the buffer object to upload data to.</param>
    /// <param name="data">The data to upload.</param>
    /// <param name="hint">The usage hint for the buffer data.</param>
    public void SetBufferData(BufferTarget target, uint buffer, float[] data, BufferHint hint);
    /// <summary>
    /// Uploads index data to the specified buffer object in the renderer, replacing its current contents with the provided index data. The target specifies that the buffer is used for element array data, and the hint indicates how the data will be used (e.g., static draw, dynamic draw). This method is used to transfer index data from CPU memory to GPU memory, enabling efficient rendering of 3D objects with indexed geometry.
    /// </summary>
    /// <param name="target">The target to upload index data to.</param>
    /// <param name="buffer">The handle to the buffer object to upload index data to.</param>
    /// <param name="data">The index data to upload.</param>
    /// <param name="hint">The usage hint for the index buffer data.</param>
    public void SetBufferData(BufferTarget target, uint buffer, uint[] data, BufferHint hint);
    /// <summary>
    /// Draws elements from the currently bound element array buffer in the renderer, using the specified count of indices. This method issues a draw call to render primitives (e.g., triangles, lines) based on the index data stored in the element array buffer, allowing for efficient rendering of 3D objects with indexed geometry. The count parameter specifies how many indices to read from the buffer, determining the number of primitives to render.
    /// </summary>
    /// <param name="index">The index of the vertex attribute to enable.</param>
    public void EnableVertexAttribute(int index);
    /// <summary>
    /// Specifies the layout of a vertex attribute in the currently bound vertex array object (VAO) in the renderer. This method defines how the vertex attribute data is organized in the buffer, including its size, stride, and offset. The index parameter specifies which vertex attribute to configure, while the size parameter indicates the number of components in the attribute (e.g., 3 for a vec3 position). The stride parameter defines the byte offset between consecutive attributes, and the offset parameter specifies the byte offset of the first component of the attribute within the buffer. This method is essential for correctly interpreting vertex data during rendering.
    /// </summary>
    /// <param name="index">The index of the vertex attribute to configure.</param>
    /// <param name="size">The number of components in the vertex attribute.</param>
    /// <param name="stride">The byte offset between consecutive attributes.</param>
    /// <param name="offset">The byte offset of the first component of the attribute within the buffer.</param>
    public void AddVertexAttribute(int index, int size, int stride, int offset);
    #endregion

    #region Texture Methods
    /// <summary>
    /// Creates a new texture object in the renderer, allocating GPU memory for storing image data. The texture can be used for mapping images onto 3D surfaces, enabling realistic rendering of materials and surfaces. This method returns a handle to the newly created texture object, which can be used in subsequent rendering operations.
    /// </summary>
    /// <returns>The handle to the newly created texture object.</returns>
    public uint CreateTexture();
    /// <summary>
    /// Deletes the specified texture object in the renderer, releasing any associated GPU resources and memory. The texture object is no longer valid after this operation, and any attempts to bind or use it will result in undefined behavior. This method is used to manage the lifecycle of texture objects, ensuring that GPU resources are properly cleaned up when they are no longer needed.
    /// </summary>
    /// <param name="texture">The texture object to delete.</param>
    public void DeleteTexture(Texture texture);
    /// <summary>
    /// Binds the specified texture object in the renderer, making it the current texture for subsequent rendering operations. The texture can be used for mapping images onto 3D surfaces, enabling realistic rendering of materials and surfaces. Binding a texture ensures that the correct image data is used when rendering objects that reference the texture, allowing for proper visual representation of materials in the scene.
    /// </summary>
    /// <param name="texture">The texture object to bind.</param>
    public void BindTexture(Texture? texture);
    /// <summary>
    /// Sets the filtering and wrapping parameters for the specified texture object in the renderer. The filtering parameters determine how the texture is sampled when it is magnified or minified, while the wrapping parameters define how texture coordinates outside the [0, 1] range are handled. This method allows for fine-tuning of texture appearance and behavior, enabling effects such as mipmapping, anisotropic filtering, and different wrapping modes (e.g., repeat, clamp).
    /// </summary>
    /// <param name="filterMode">The filtering mode to set.</param>
    /// <param name="mipmapFilter">The mipmap filtering mode to set.</param>
    public void SetTextureMinFilter(TextureFilter filterMode, MipmapFilter mipmapFilter);
    /// <summary>
    /// Sets the magnification filtering mode for the specified texture object in the renderer. The magnification filter determines how the texture is sampled when it is displayed larger than its original size, affecting the visual quality of the texture when viewed up close. This method allows for fine-tuning of texture appearance during magnification, enabling effects such as bilinear or trilinear filtering for smoother results.
    /// </summary>
    /// <param name="filterMode">The magnification filtering mode to set.</param>
    public void SetTextureMagFilter(TextureFilter filterMode);
    /// <summary>
    /// Sets the horizontal wrapping mode for the specified texture object in the renderer. The wrapping mode determines how texture coordinates outside the [0, 1] range are handled along the horizontal axis (U direction). This method allows for fine-tuning of texture behavior, enabling effects such as repeating, clamping, or mirroring of textures when they are applied to 3D surfaces.
    /// </summary>
    /// <param name="wrapModeH">The horizontal wrapping mode to set.</param>
    public void SetTextureWrapModeH(TextureWrap wrapModeH);
    /// <summary>
    /// Sets the vertical wrapping mode for the specified texture object in the renderer. The wrapping mode determines how texture coordinates outside the [0, 1] range are handled along the vertical axis (V direction). This method allows for fine-tuning of texture behavior, enabling effects such as repeating, clamping, or mirroring of textures when they are applied to 3D surfaces.
    /// </summary>
    /// <param name="wrapModeV">The vertical wrapping mode to set.</param>
    public void SetTextureWrapModeV(TextureWrap wrapModeV);
    /// <summary>
    /// Sets the anisotropic filtering level for the specified texture object in the renderer. Anisotropic filtering improves the quality of textures viewed at oblique angles, reducing blurriness and preserving detail. This method allows for fine-tuning of texture appearance, enabling higher levels of anisotropic filtering for better visual fidelity in 3D scenes.
    /// </summary>
    /// <param name="anisotropicFilter">The anisotropic filtering level to set.</param>
    public void SetTextureAnisotropicFilter(AnisotropicFilter anisotropicFilter);
    /// <summary>
    /// Uploads image data to the specified texture object in the renderer, replacing its current contents with the provided image data. The texture can be used for mapping images onto 3D surfaces, enabling realistic rendering of materials and surfaces. This method transfers image data from CPU memory to GPU memory, allowing for efficient rendering of textures in 3D scenes.
    /// </summary>
    /// <param name="texture">The texture object to upload image data to.</param>
    public void UploadTexture(Texture texture);
    /// <summary>
    /// Generates mipmaps for the currently bound texture object in the renderer. Mipmaps are pre-calculated, optimized sequences of images that represent the texture at progressively lower resolutions. Generating mipmaps improves rendering performance and visual quality when textures are viewed at a distance or at smaller sizes, reducing aliasing and providing smoother transitions between different levels of detail. This method should be called after uploading texture data to ensure that mipmaps are available for rendering.
    /// </summary>
    public void GenerateMipmaps();
    #endregion

    #region Shader Methods
    /// <summary>
    /// Creates a new shader object of the specified type in the renderer, allocating GPU resources for compiling and executing shader code. The shader can be used to define custom rendering behavior, including vertex transformations, fragment shading, and other programmable effects. This method returns a handle to the newly created shader object, which can be used in subsequent rendering operations.
    /// </summary>
    /// <param name="shaderType">The type of shader to create.</param>
    /// <returns>The handle to the newly created shader object.</returns>
    public uint CreateShader(ShaderType shaderType);
    /// <summary>
    /// Deletes the specified shader object in the renderer, releasing any associated GPU resources and memory. The shader object is no longer valid after this operation, and any attempts to bind or use it will result in undefined behavior. This method is used to manage the lifecycle of shader objects, ensuring that GPU resources are properly cleaned up when they are no longer needed.
    /// </summary>
    /// <param name="shader">The shader object to delete.</param>
    public void DeleteShader(uint shader);
    /// <summary>
    /// Sets the source code for the specified shader object in the renderer, replacing its current source code with the provided shader code. The shader source code defines the behavior of the shader, including vertex transformations, fragment shading, and other programmable effects. This method allows for dynamic modification of shader behavior by providing new source code for compilation and execution.
    /// </summary>
    /// <param name="shader">The shader object for which to set source code.</param>
    /// <param name="source">The shader source code to set.</param>
    public void SetShaderSource(uint shader, string source);
    /// <summary>
    /// Compiles the specified shader object in the renderer, translating its source code into executable GPU instructions. The compilation process checks for syntax errors, validates the shader code, and prepares it for execution during rendering. This method returns a boolean indicating whether the compilation was successful, allowing for error handling and debugging of shader code.
    /// </summary>
    /// <param name="shader">The shader object to compile.</param>
    /// <returns>A boolean indicating whether the compilation was successful.</returns>
    public bool CompileShader(uint shader);
    /// <summary>
    /// Retrieves the compilation log for the specified shader object in the renderer, providing information about any errors, warnings, or messages generated during the compilation process. The shader log can be used for debugging and troubleshooting shader code, allowing developers to identify issues and improve shader performance. This method returns a string containing the compilation log for the specified shader.
    /// </summary>
    /// <param name="shader">The shader object for which to retrieve the compilation log.</param>
    /// <returns>A string containing the compilation log for the specified shader.</returns>
    public string GetShaderLog(uint shader);
    /// <summary>
    /// Creates a new shader program in the renderer, which can be used to link multiple shader objects together for rendering. The shader program encapsulates the combined behavior of vertex, fragment, and other shader stages, allowing for complex rendering effects. This method returns a handle to the newly created shader program, which can be used in subsequent rendering operations.
    /// </summary>
    /// <returns>A handle to the newly created shader program.</returns>
    public uint CreateShaderProgram();
    /// <summary>
    /// Deletes the specified shader program in the renderer, releasing any associated GPU resources and memory. The shader program is no longer valid after this operation, and any attempts to use it will result in undefined behavior. This method is used to manage the lifecycle of shader programs, ensuring that GPU resources are properly cleaned up when they are no longer needed.
    /// </summary>
    /// <param name="program">The shader program to delete.</param>
    public void DeleteShaderProgram(uint program);
    /// <summary>
    /// Attaches a shader object to the specified shader program in the renderer, allowing the shader to be linked and executed as part of the program. The shader program can consist of multiple shader stages (e.g., vertex, fragment), and attaching shaders enables the combination of their behavior for rendering. This method is essential for creating complex rendering effects by linking different shader objects together within a single program.
    /// </summary>
    /// <param name="program">The shader program to which to attach the shader.</param>
    /// <param name="shader">The shader object to attach.</param>
    public void AttachShader(uint program, uint shader);
    /// <summary>
    /// Links the specified shader program in the renderer, combining the attached shader objects into a single executable program. The linking process resolves references between shader stages, validates the program, and prepares it for execution during rendering. This method returns a boolean indicating whether the linking was successful, allowing for error handling and debugging of shader programs.
    /// </summary>
    /// <param name="program">The shader program to link.</param>
    /// <returns>A boolean indicating whether the linking was successful.</returns>
    public bool LinkShaderProgram(uint program);
    /// <summary>
    /// Retrieves the linking log for the specified shader program in the renderer, providing information about any errors, warnings, or messages generated during the linking process. The program log can be used for debugging and troubleshooting shader programs, allowing developers to identify issues and improve rendering performance. This method returns a string containing the linking log for the specified shader program.
    /// </summary>
    /// <param name="program">The shader program for which to retrieve the linking log.</param>
    /// <returns>A string containing the linking log for the specified shader program.</returns>
    public string GetShaderProgramLog(uint program);
    /// <summary>
    /// Sets the specified shader program as the current program for rendering in the renderer. The shader program defines the combined behavior of vertex, fragment, and other shader stages, allowing for complex rendering effects. This method ensures that subsequent rendering operations use the specified shader program, enabling the execution of its attached shaders and their associated behavior during drawing.
    /// </summary>
    /// <param name="program">The shader program to use.</param>
    public void UseShaderProgram(uint program);
    /// <summary>
    /// Retrieves the location of a uniform variable within the specified shader program in the renderer. Uniform variables are used to pass data from the CPU to the GPU, allowing for dynamic control of shader behavior during rendering. This method returns an integer representing the location of the uniform variable, which can be used in subsequent calls to set its value.
    /// </summary>
    /// <param name="program">The shader program containing the uniform variable.</param>
    /// <param name="name">The name of the uniform variable.</param>
    /// <returns>An integer representing the location of the uniform variable.</returns>
    public int GetUniformLocation(uint program, string name);
    /// <summary>
    /// Sets the value of a uniform variable in the currently active shader program in the renderer. Uniform variables are used to pass data from the CPU to the GPU, allowing for dynamic control of shader behavior during rendering. This method allows for setting various types of uniform data, such as matrices, integers, and colors, enabling flexible and customizable rendering effects based on application logic and user input.
    /// </summary>
    /// <param name="location">The location of the uniform variable.</param>
    /// <param name="data">The data to set.</param>
    public void SetUniformMatrix(int location, Matrix4x4 data);
    /// <summary>
    /// Sets the value of an integer uniform variable in the currently active shader program in the renderer. Uniform variables are used to pass data from the CPU to the GPU, allowing for dynamic control of shader behavior during rendering. This method allows for setting integer values for uniform variables, enabling flexible and customizable rendering effects based on application logic and user input.
    /// </summary>
    /// <param name="location">The location of the uniform variable.</param>
    /// <param name="data">The integer data to set.</param>
    public void SetUniformInt(int location, int data);
    /// <summary>
    /// Sets the value of a color uniform variable in the currently active shader program in the renderer. Uniform variables are used to pass data from the CPU to the GPU, allowing for dynamic control of shader behavior during rendering. This method allows for setting color values for uniform variables, enabling flexible and customizable rendering effects based on application logic and user input.
    /// </summary>
    /// <param name="location">The location of the uniform variable.</param>
    /// <param name="color">The color data to set.</param>
    public void SetUniformColor(int location, Color color);
    #endregion

    #region Drawing Methods
    /// <summary>
    /// Clears the color buffer of the renderer with the specified color, effectively resetting the framebuffer to a uniform color. This method is typically called at the beginning of a rendering frame to prepare the framebuffer for drawing new content, ensuring that any previous frame's data is removed and replaced with the specified clear color. The clear color can be used to set the background color of the scene or to create visual effects by blending with subsequent rendering operations.
    /// </summary>
    /// <param name="color"></param>
    public void ClearColor(Color color);
    /// <summary>
    /// Clears the color buffer of the renderer, effectively resetting the framebuffer to a default state. This method is typically called at the beginning of a rendering frame to prepare the framebuffer for drawing new content, ensuring that any previous frame's data is removed. Clearing the color buffer is essential for maintaining visual clarity and preventing artifacts from previous frames from affecting the current rendering output.
    /// </summary>
    public void ClearColorBuffer();
    /// <summary>
    /// Clears the depth buffer of the renderer, effectively resetting the depth information used for depth testing during rendering. This method is typically called at the beginning of a rendering frame to prepare the depth buffer for new content, ensuring that any previous frame's depth data is removed. Clearing the depth buffer is essential for maintaining correct occlusion and visibility of 3D objects in the scene, allowing for accurate rendering of overlapping geometry based on their distances from the camera.
    /// </summary>
    public void ClearDepthBuffer();
    /// <summary>
    /// Draws elements from the currently bound element array buffer in the renderer, using the specified count of indices. This method issues a draw call to render primitives (e.g., triangles, lines) based on the index data stored in the element array buffer, allowing for efficient rendering of 3D objects with indexed geometry. The count parameter specifies how many indices to read from the buffer, determining the number of primitives to render.
    /// </summary>
    /// <param name="count">The number of indices to draw.</param>
    public void DrawElements(int count);
    #endregion
}