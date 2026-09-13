using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace odl3d;

/// <summary>
/// Minimal dynamic bindings for the FreeType native library (face loading, metrics and glyph rasterization only).
/// Field offsets below are computed manually at class-init time instead of via Marshal.PtrToStructure, because
/// FT_Long/FT_Pos/FT_Fixed/FT_ULong are plain C "long": 4 bytes on Windows x64 (LLP64) but 8 bytes on Linux/macOS
/// x64 (LP64). A struct hardcoding these as 8-byte fields works on Unix but silently misaligns every field on
/// Windows, reading garbage (and eventually dereferencing an invalid pointer). Only fields actually read by
/// Font.cs are computed; trailing native fields are simply never touched.
/// </summary>
internal static unsafe class FT
{
#pragma warning disable CS8618 // delegate fields are assigned in Load(), not the static constructor
    public const int FT_LOAD_DEFAULT = 0x0;
    public const uint FT_KERNING_DEFAULT = 0;
    public const int FT_RENDER_MODE_NORMAL = 0;

    /// <summary>Set on FT_Outline tags whose point lies on the curve itself rather than being a control point.</summary>
    public const byte FT_CURVE_TAG_ON = 0x1;
    /// <summary>Set on FT_Outline tags for the control points of a cubic (third-order) Bézier; clear means conic.</summary>
    public const byte FT_CURVE_TAG_CUBIC = 0x2;

    // Size, in bytes, of FT_Long/FT_Pos/FT_Fixed/FT_ULong on this platform's FreeType build.
    private static readonly int LONG = OperatingSystem.IsWindows() ? 4 : 8;
    private const int PTR = 8; // x64 pointer size, true on every supported platform here
    private const int INT = 4;
    private const int SHORT = 2;

    /// <summary>Computes sequential, natural-alignment field offsets, matching typical C compiler layout rules.</summary>
    private struct Layout
    {
        private int _offset;
        private int _maxAlign;

        public int Add(int size, int align)
        {
            _maxAlign = Math.Max(_maxAlign, align);
            _offset = AlignUp(_offset, align);
            int at = _offset;
            _offset += size;
            return at;
        }

        public readonly int Offset => _offset;
        public readonly int End() => AlignUp(_offset, Math.Max(_maxAlign, 1));
        private static int AlignUp(int v, int a) => (v + a - 1) / a * a;
    }

    // ---- FT_Bitmap ----
    private static readonly int Bitmap_rows, Bitmap_width, Bitmap_pitch, Bitmap_buffer, Bitmap_SIZE;

    // ---- FT_GlyphSlotRec (prefix through 'outline') ----
    private static readonly int Slot_advance, Slot_bitmap, Slot_bitmap_left, Slot_bitmap_top, Slot_outline;

    // ---- FT_FaceRec (prefix through 'size') ----
    private static readonly int Face_underline_position, Face_underline_thickness, Face_glyph, Face_size;

    // ---- FT_SizeRec / nested FT_Size_Metrics ----
    private static readonly int Size_metrics, SizeMetrics_ascender, SizeMetrics_descender, SizeMetrics_height;

    // ---- FT_Outline ----
    private static readonly int Outline_n_contours, Outline_n_points, Outline_points, Outline_tags, Outline_contours;

    static FT()
    {
        Layout bm = new Layout();
        Bitmap_rows = bm.Add(INT, INT);
        Bitmap_width = bm.Add(INT, INT);
        Bitmap_pitch = bm.Add(INT, INT);
        Bitmap_buffer = bm.Add(PTR, PTR);
        bm.Add(SHORT, SHORT); // num_grays
        bm.Add(1, 1); // pixel_mode
        bm.Add(1, 1); // palette_mode
        bm.Add(PTR, PTR); // palette
        Bitmap_SIZE = bm.End();

        int glyphMetricsSize = 8 * LONG; // FT_Glyph_Metrics: 8 FT_Pos fields

        Layout gs = new Layout();
        gs.Add(PTR, PTR); // library
        gs.Add(PTR, PTR); // face
        gs.Add(PTR, PTR); // next
        gs.Add(INT, INT); // glyph_index
        gs.Add(PTR, PTR); // generic.data
        gs.Add(PTR, PTR); // generic.finalizer
        gs.Add(glyphMetricsSize, LONG); // metrics
        gs.Add(LONG, LONG); // linearHoriAdvance
        gs.Add(LONG, LONG); // linearVertAdvance
        Slot_advance = gs.Add(2 * LONG, LONG); // FT_Vector advance
        gs.Add(INT, INT); // format
        Slot_bitmap = gs.Add(Bitmap_SIZE, PTR);
        Slot_bitmap_left = gs.Add(INT, INT);
        Slot_bitmap_top = gs.Add(INT, INT);
        Slot_outline = gs.Offset; // FT_Outline starts here; its first field only needs 2-byte alignment

        Layout fr = new Layout();
        fr.Add(LONG, LONG); // num_faces
        fr.Add(LONG, LONG); // face_index
        fr.Add(LONG, LONG); // face_flags
        fr.Add(LONG, LONG); // style_flags
        fr.Add(LONG, LONG); // num_glyphs
        fr.Add(PTR, PTR); // family_name
        fr.Add(PTR, PTR); // style_name
        fr.Add(INT, INT); // num_fixed_sizes
        fr.Add(PTR, PTR); // available_sizes
        fr.Add(INT, INT); // num_charmaps
        fr.Add(PTR, PTR); // charmaps
        fr.Add(PTR, PTR); // generic.data
        fr.Add(PTR, PTR); // generic.finalizer
        fr.Add(4 * LONG, LONG); // bbox
        fr.Add(SHORT, SHORT); // units_per_EM
        fr.Add(SHORT, SHORT); // ascender
        fr.Add(SHORT, SHORT); // descender
        fr.Add(SHORT, SHORT); // height
        fr.Add(SHORT, SHORT); // max_advance_width
        fr.Add(SHORT, SHORT); // max_advance_height
        Face_underline_position = fr.Add(SHORT, SHORT);
        Face_underline_thickness = fr.Add(SHORT, SHORT);
        Face_glyph = fr.Add(PTR, PTR);
        Face_size = fr.Add(PTR, PTR);

        Layout szm = new Layout();
        szm.Add(SHORT, SHORT); // x_ppem
        szm.Add(SHORT, SHORT); // y_ppem
        szm.Add(LONG, LONG); // x_scale
        szm.Add(LONG, LONG); // y_scale
        SizeMetrics_ascender = szm.Add(LONG, LONG);
        SizeMetrics_descender = szm.Add(LONG, LONG);
        SizeMetrics_height = szm.Add(LONG, LONG);
        szm.Add(LONG, LONG); // max_advance
        int sizeMetricsSize = szm.End();

        Layout sr = new Layout();
        sr.Add(PTR, PTR); // face
        sr.Add(PTR, PTR); // generic.data
        sr.Add(PTR, PTR); // generic.finalizer
        Size_metrics = sr.Add(sizeMetricsSize, LONG);

        Layout ol = new Layout();
        Outline_n_contours = ol.Add(SHORT, SHORT);
        Outline_n_points = ol.Add(SHORT, SHORT);
        Outline_points = ol.Add(PTR, PTR);
        Outline_tags = ol.Add(PTR, PTR);
        Outline_contours = ol.Add(PTR, PTR);
    }

    private static long ReadFTLong(IntPtr p) => LONG == 4 ? *(int*)p : *(long*)p;
    private static void WriteFTLong(IntPtr p, long value)
    {
        if (LONG == 4) *(int*)p = (int)value;
        else *(long*)p = value;
    }

    public static (int Width, int Height, int Pitch, IntPtr Buffer) GetBitmap(IntPtr glyphSlot)
    {
        IntPtr b = glyphSlot + Slot_bitmap;
        int width = *(int*)(b + Bitmap_width);
        int height = *(int*)(b + Bitmap_rows);
        int pitch = *(int*)(b + Bitmap_pitch);
        IntPtr buffer = *(IntPtr*)(b + Bitmap_buffer);
        return (width, height, pitch, buffer);
    }

    public static int GetBitmapLeft(IntPtr glyphSlot) => *(int*)(glyphSlot + Slot_bitmap_left);
    public static int GetBitmapTop(IntPtr glyphSlot) => *(int*)(glyphSlot + Slot_bitmap_top);
    public static float GetAdvanceX(IntPtr glyphSlot) => ReadFTLong(glyphSlot + Slot_advance) / 64f;
    public static IntPtr GetOutlinePtr(IntPtr glyphSlot) => glyphSlot + Slot_outline;

    public static int GetOutlineContourCount(IntPtr outline) => *(ushort*)(outline + Outline_n_contours);
    public static IntPtr GetOutlinePoints(IntPtr outline) => *(IntPtr*)(outline + Outline_points);
    public static IntPtr GetOutlineTags(IntPtr outline) => *(IntPtr*)(outline + Outline_tags);
    public static IntPtr GetOutlineContours(IntPtr outline) => *(IntPtr*)(outline + Outline_contours);

    /// <summary>Index of the last point belonging to contour <paramref name="index"/> of an outline.</summary>
    public static int GetOutlineContourEnd(IntPtr contours, int index) => *((ushort*)contours + index);

    /// <summary>Reads an outline point, converting FreeType's 26.6 fixed-point coordinates to pixels (y up, origin on the baseline).</summary>
    public static Vector2 GetOutlinePoint(IntPtr points, int index)
    {
        IntPtr point = points + index * 2 * LONG;
        return new Vector2(ReadFTLong(point) / 64f, ReadFTLong(point + LONG) / 64f);
    }

    public static byte GetOutlineTag(IntPtr tags, int index) => *((byte*)tags + index);

    public static IntPtr GetFaceGlyphSlot(IntPtr face) => *(IntPtr*)(face + Face_glyph);
    public static IntPtr GetFaceSize(IntPtr face) => *(IntPtr*)(face + Face_size);
    public static float GetFaceUnderlinePosition(IntPtr face) => *(short*)(face + Face_underline_position) / 64f;
    public static float GetFaceUnderlineThickness(IntPtr face) => *(short*)(face + Face_underline_thickness) / 64f;

    public static (float Ascender, float Descender, float Height) GetSizeMetrics(IntPtr size)
    {
        IntPtr m = size + Size_metrics;
        float ascender = ReadFTLong(m + SizeMetrics_ascender) / 64f;
        float descender = ReadFTLong(m + SizeMetrics_descender) / 64f;
        float height = ReadFTLong(m + SizeMetrics_height) / 64f;
        return (ascender, descender, height);
    }

    /// <summary>
    /// Writes a 4x FT_Fixed (16.16) shear/scale matrix into a caller-allocated buffer of at least
    /// <see cref="MatrixBufferSize"/> bytes, suitable for passing to FT_Set_Transform.
    /// </summary>
    public static void WriteMatrix(IntPtr buffer, double xx, double xy, double yx, double yy)
    {
        WriteFTLong(buffer, (long)(xx * 65536));
        WriteFTLong(buffer + LONG, (long)(xy * 65536));
        WriteFTLong(buffer + 2 * LONG, (long)(yx * 65536));
        WriteFTLong(buffer + 3 * LONG, (long)(yy * 65536));
    }

    public static int MatrixBufferSize => 4 * LONG;
    public static int KerningBufferSize => 2 * LONG;

    /// <summary>Reads the 'x' (horizontal) component out of an FT_Vector buffer written by FT_Get_Kerning.</summary>
    public static float ReadKerningX(IntPtr buffer) => ReadFTLong(buffer) / 64f;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_FT_Init_FreeType(out IntPtr alibrary);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_FT_Done_FreeType(IntPtr library);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_FT_New_Face(IntPtr library, [MarshalAs(UnmanagedType.LPStr)] string filepathname, long face_index, out IntPtr aface);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_FT_Done_Face(IntPtr face);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_FT_Set_Pixel_Sizes(IntPtr face, uint pixel_width, uint pixel_height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint d_FT_Get_Char_Index(IntPtr face, nuint charcode);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_FT_Load_Glyph(IntPtr face, uint glyph_index, int load_flags);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_FT_Render_Glyph(IntPtr slot, int render_mode);
    // akerning is a raw pointer to a manually-packed FT_Vector buffer (see KerningBufferSize/ReadKerningX).
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_FT_Get_Kerning(IntPtr face, uint left_glyph, uint right_glyph, uint kern_mode, IntPtr akerning);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_FT_Set_Transform(IntPtr face, IntPtr matrix, IntPtr delta);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_FT_Outline_Embolden(IntPtr outline, long strength);

    public static d_FT_Init_FreeType FT_Init_FreeType;
    public static d_FT_Done_FreeType FT_Done_FreeType;
    public static d_FT_New_Face FT_New_Face;
    public static d_FT_Done_Face FT_Done_Face;
    public static d_FT_Set_Pixel_Sizes FT_Set_Pixel_Sizes;
    public static d_FT_Get_Char_Index FT_Get_Char_Index;
    public static d_FT_Load_Glyph FT_Load_Glyph;
    public static d_FT_Render_Glyph FT_Render_Glyph;
    public static d_FT_Get_Kerning FT_Get_Kerning;
    public static d_FT_Set_Transform FT_Set_Transform;
    public static d_FT_Outline_Embolden FT_Outline_Embolden;
#pragma warning restore CS8618

    private static IntPtr _library;

    public static bool Loaded { get; private set; }

    public static void Load()
    {
        if (Loaded) return;

        // Candidate native library names per OS; FreeType is not bundled and must be resolvable at runtime.
        string[] candidates =
            OperatingSystem.IsWindows() ? ["freetype.dll", "freetype"] :
            OperatingSystem.IsMacOS() ? ["libfreetype.6.dylib", "libfreetype.dylib"] :
                                         ["libfreetype.so.6", "libfreetype.so"];

        foreach (string candidate in candidates)
        {
            if (NativeLibrary.TryLoad("bin/" + candidate, out _library)) break;
        }
        if (_library == IntPtr.Zero)
            throw new DllNotFoundException("Could not locate the FreeType native library (freetype.dll / libfreetype.so.6 / libfreetype.6.dylib). Install it or place it next to the executable.");

        FT_Init_FreeType = GetFunction<d_FT_Init_FreeType>("FT_Init_FreeType");
        FT_Done_FreeType = GetFunction<d_FT_Done_FreeType>("FT_Done_FreeType");
        FT_New_Face = GetFunction<d_FT_New_Face>("FT_New_Face");
        FT_Done_Face = GetFunction<d_FT_Done_Face>("FT_Done_Face");
        FT_Set_Pixel_Sizes = GetFunction<d_FT_Set_Pixel_Sizes>("FT_Set_Pixel_Sizes");
        FT_Get_Char_Index = GetFunction<d_FT_Get_Char_Index>("FT_Get_Char_Index");
        FT_Load_Glyph = GetFunction<d_FT_Load_Glyph>("FT_Load_Glyph");
        FT_Render_Glyph = GetFunction<d_FT_Render_Glyph>("FT_Render_Glyph");
        FT_Get_Kerning = GetFunction<d_FT_Get_Kerning>("FT_Get_Kerning");
        FT_Set_Transform = GetFunction<d_FT_Set_Transform>("FT_Set_Transform");
        FT_Outline_Embolden = GetFunction<d_FT_Outline_Embolden>("FT_Outline_Embolden");

        Loaded = true;
    }

    private static TDelegate GetFunction<TDelegate>(string name) where TDelegate : Delegate
    {
        if (!NativeLibrary.TryGetExport(_library, name, out IntPtr ptr))
            throw new EntryPointNotFoundException($"Could not find FreeType function '{name}'.");
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr);
    }
}
