using System;
using System.Runtime.InteropServices;

namespace odl3d.Renderer;

internal static partial class Metal
{
    /// <summary>
    /// Represents a Metal texture, which encapsulates the image data used in rendering.
    /// </summary>
    public sealed class Texture : ObjCObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Texture"/> class with the specified handle.
        /// </summary>
        /// <param name="handle">The handle to the native Metal texture object.</param>
        public Texture(IntPtr handle) : base(handle, true) { }

        /// <summary>
        /// Uploads the specified image data to the texture at the given mipmap level.
        /// </summary>
        /// <param name="bytes">A pointer to the image data to upload.</param>
        /// <param name="mipLevel">The mipmap level to which the data should be uploaded.</param>
        /// <param name="bytesPerRow">The number of bytes per row of the image data.</param>
        /// <param name="width">The width of the image data.</param>
        /// <param name="height">The height of the image data.</param>
        public void Upload(IntPtr bytes, nuint mipLevel, nuint bytesPerRow, nuint width, nuint height)
        {
            MTLRegion region = new MTLRegion
            {
                X = 0,
                Y = 0,
                Z = 0,
                Width = width,
                Height = height,
                Depth = 1
            };
            Send("replaceRegion:mipmapLevel:withBytes:bytesPerRow:", region, mipLevel, bytes, bytesPerRow);
        }

        /// <summary>
        /// Uploads the specified image data to the texture at the given mipmap level.
        /// </summary>
        /// <param name="bytes">The image data to upload.</param>
        /// <param name="mipLevel">The mipmap level to which the data should be uploaded.</param>
        /// <param name="bytesPerRow">The number of bytes per row of the image data.</param>
        /// <param name="width">The width of the image data.</param>
        /// <param name="height">The height of the image data.</param>
        public unsafe void Upload(byte[] bytes, nuint mipLevel, nuint bytesPerRow, nuint width, nuint height)
        {
            fixed (byte* bytePtr = bytes) Upload((nint) bytePtr, mipLevel, bytesPerRow, width, height);
        }

        /// <summary>
        /// Copies the contents of mip level 0 into the specified buffer. The texture must be CPU-accessible and
        /// any GPU writes to it must have completed and been synchronized.
        /// </summary>
        /// <param name="destination">The buffer to copy into; must hold at least bytesPerRow * height bytes.</param>
        /// <param name="bytesPerRow">The number of bytes per row in the destination buffer.</param>
        /// <param name="width">The width of the region to copy.</param>
        /// <param name="height">The height of the region to copy.</param>
        public unsafe void GetBytes(byte[] destination, nuint bytesPerRow, nuint width, nuint height)
        {
            MTLRegion region = new MTLRegion { Width = width, Height = height, Depth = 1 };
            fixed (byte* ptr = destination)
                Send("getBytes:bytesPerRow:fromRegion:mipmapLevel:", (nint) ptr, bytesPerRow, region, 0);
        }
    }
}
