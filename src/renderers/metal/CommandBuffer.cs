using System;

namespace odl3d.Renderers;

public static partial class MetalNew
{
    public class CommandBuffer : ObjCObject
    {
        public CommandBuffer(IntPtr handle) : base(handle) { }

        public CommandEncoder CreateRenderCommandEncoder(RenderPassDescriptor descriptor) =>
            new CommandEncoder(Send("renderCommandEncoderWithDescriptor:", descriptor));

        public CommandEncoder RenderCommandEncoder(RenderPassDescriptor descriptor) =>
            CreateRenderCommandEncoder(descriptor);

        public void PresentDrawable(Drawable drawable) => Send("presentDrawable:", drawable);

        public void Present(Drawable drawable) => PresentDrawable(drawable);

        public void Commit() => Send("commit");
    }
}