using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal render pipeline state, which encapsulates the compiled state of a render pipeline.
    /// </summary>
    public sealed class RenderPipelineState : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPipelineState"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native Metal render pipeline state object.</param>
        public RenderPipelineState(IntPtr handle) : base(handle, true) { }
    }
}
