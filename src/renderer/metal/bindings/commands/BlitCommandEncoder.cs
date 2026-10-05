using System;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal blit command encoder, which is used to encode commands for copying and generating mipmaps for textures.
    /// </summary>
    public sealed class BlitCommandEncoder : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the BlitCommandEncoder class with the specified native handle.
        /// </summary>
        /// <param name="handle"></param>
        public BlitCommandEncoder(IntPtr handle) : base(handle) { }

        /// <summary>
        /// Generates mipmaps for the specified texture.
        /// </summary>
        /// <param name="texture">The texture for which to generate mipmaps.</param>
        public void GenerateMipmaps(Texture texture) =>
            Send("generateMipmapsForTexture:", texture);

        /// <summary>
        /// Copies GPU-side changes of a managed resource back to its CPU-accessible copy.
        /// </summary>
        /// <param name="resource">The managed buffer or texture to synchronize.</param>
        public void SynchronizeResource(ObjCObject resource) =>
            Send("synchronizeResource:", resource);

        /// <summary>
        /// Ends the encoding of commands for the blit command encoder. After calling this method, no further commands can be encoded with this encoder.
        /// </summary>
        public void EndEncoding() => Send("endEncoding");
    }
}
