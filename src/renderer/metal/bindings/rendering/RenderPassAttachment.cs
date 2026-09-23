using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a render pass attachment in Metal, which can be a color, depth, or stencil attachment.
    /// </summary>
    public abstract class RenderPassAttachment : ObjCObject
    {
        /// <summary>
        /// Gets or sets the texture associated with this render pass attachment.
        /// </summary>
        public ObjCObject Texture
        {
            get => Get("texture");
            set => Send("setTexture:", value);
        }

        /// <summary>
        /// Gets or sets the load action for this render pass attachment.
        /// </summary>
        public LoadAction LoadAction
        {
            get => (LoadAction) GetUInt32("loadAction");
            set => Send("setLoadAction:", (nuint) value);
        }

        /// <summary>
        /// Gets or sets the store action for this render pass attachment.
        /// </summary>
        public StoreAction StoreAction
        {
            get => (StoreAction) GetUInt32("storeAction");
            set => Send("setStoreAction:", (nuint) value);
        }

        /// <summary>
        /// Initializes a new instance of the RenderPassAttachment class with the specified native handle.
        /// </summary>
        /// <param name="handle">The native handle for the render pass attachment.</param>
        public RenderPassAttachment(IntPtr handle) : base(handle) { }
    }
}