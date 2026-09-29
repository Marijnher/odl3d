using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal command buffer, which is used to encode and commit commands to the GPU.
    /// </summary>
    public sealed class CommandBuffer : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the CommandBuffer class with the specified native handle.
        /// </summary>
        /// <param name="handle">The native handle of the Metal command buffer.</param>
        public CommandBuffer(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Creates a render command encoder for the specified render pass descriptor. The render command encoder is used to encode rendering commands for the GPU.
        /// </summary>
        /// <param name="descriptor">The render pass descriptor that defines the rendering configuration for the command encoder.</param>
        /// <returns>A new instance of the CommandEncoder class for encoding rendering commands.</returns>
        public CommandEncoder CreateRenderCommandEncoder(RenderPassDescriptor descriptor) =>
            new CommandEncoder(Send("renderCommandEncoderWithDescriptor:", descriptor));

        /// <summary>
        /// Creates a render command encoder for the specified render pass descriptor. This is an alternative method to CreateRenderCommandEncoder and provides the same functionality.
        /// </summary>
        /// <param name="descriptor">The render pass descriptor that defines the rendering configuration for the command encoder.</param>
        /// <returns>A new instance of the CommandEncoder class for encoding rendering commands.</returns>
        public CommandEncoder RenderCommandEncoder(RenderPassDescriptor descriptor) =>
            CreateRenderCommandEncoder(descriptor);

        /// <summary>
        /// Creates a blit command encoder for encoding copy and mipmap generation commands.
        /// </summary>
        /// <returns>A new instance of the BlitCommandEncoder class for encoding blit commands.</returns>
        public BlitCommandEncoder CreateBlitCommandEncoder() =>
            new BlitCommandEncoder(Send("blitCommandEncoder"));
        
        /// <summary>
        /// Presents the specified drawable to the screen. This schedules the drawable to be displayed once all previously encoded commands have been executed.
        /// </summary>
        /// <param name="drawable">The drawable to present.</param>
        public void Present(Drawable drawable) => Send("presentDrawable:", drawable);

        /// <summary>
        /// Commits the command buffer, scheduling it for execution on the GPU.
        /// </summary>
        public void Commit() => Send("commit");

        /// <summary>
        /// Blocks the calling thread until the command buffer has finished executing on the GPU.
        /// </summary>
        public void WaitUntilCompleted() => Send("waitUntilCompleted");
    }
}