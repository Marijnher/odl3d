using System;

namespace odl3d.Renderers;

public static partial class Metal
{
    public sealed class CommandBuffer : ObjCObject
    {
        public CommandBuffer(IntPtr handle) : base(handle) { }

        public CommandEncoder CreateRenderCommandEncoder(RenderPassDescriptor descriptor) =>
            new CommandEncoder(Send("renderCommandEncoderWithDescriptor:", descriptor));

        public CommandEncoder RenderCommandEncoder(RenderPassDescriptor descriptor) =>
            CreateRenderCommandEncoder(descriptor);

        public BlitCommandEncoder CreateBlitCommandEncoder() =>
            new BlitCommandEncoder(Send("blitCommandEncoder"));
        
        public void Present(Drawable drawable) => Send("presentDrawable:", drawable);

        public void Commit() => Send("commit");

        public void WaitUntilCompleted() => Send("waitUntilCompleted");
    }
}