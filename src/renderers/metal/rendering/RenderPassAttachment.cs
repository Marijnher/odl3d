using System;

namespace odl3d.Renderer;

public static partial class Metal
{
    public abstract class RenderPassAttachment : ObjCObject
    {
        public RenderPassAttachment(IntPtr handle) : base(handle) { }

        public void SetTexture(ObjCObject texture) => Send("setTexture:", texture);
        public void SetLoadAction(LoadAction action) => Send("setLoadAction:", (nuint) action);
        public void SetStoreAction(StoreAction action) => Send("setStoreAction:", (nuint) action);
    }
}