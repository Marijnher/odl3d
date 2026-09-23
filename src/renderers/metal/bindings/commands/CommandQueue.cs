using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal command queue, which is used to submit command buffers to the GPU for execution.
    /// </summary>
    public sealed class CommandQueue : ObjCObject
    {
        /// <summary>
        /// Gets or sets the label for the command queue. The label is a human-readable string used for debugging and profiling purposes.
        /// </summary>
        public string? Label
        {
            get => GetStringOrNull("label");
            set => Send("setLabel:" , value);
        }

        /// <summary>
        /// Initializes a new instance of the CommandQueue class with the specified native handle and ownership flag.
        /// </summary>
        /// <param name="handle">The native handle of the Metal command queue.</param>
        /// <param name="ownsNativeObject">A flag indicating whether the CommandQueue instance owns the native object.</param>
        public CommandQueue(IntPtr handle, bool ownsNativeObject = false) : base(handle, ownsNativeObject) { }

        /// <summary>
        /// Creates a new command buffer from the command queue. The command buffer can be used to encode rendering commands for the GPU.
        /// </summary>
        /// <returns>A new CommandBuffer instance associated with the command queue.</returns>
        public CommandBuffer CreateCommandBuffer() => Get<CommandBuffer>("commandBuffer");
    }
}