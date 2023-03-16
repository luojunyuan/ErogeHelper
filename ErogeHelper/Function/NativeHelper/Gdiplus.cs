using System.Runtime.InteropServices;
using Vanara;
using Vanara.PInvoke;

namespace ErogeHelper.Function.NativeHelper;

internal static class Gdiplus
{
    /// <summary>
    /// <para>
    /// The <see cref="GdiplusStartup"/> function initializes Windows GDI+.
    /// Call <see cref="GdiplusStartup"/> before making any other GDI+ calls, and call <see cref="GdiplusShutdown"/> when you have finished using GDI+.
    /// </para>
    /// <para>
    /// From: <see href="https://docs.microsoft.com/zh-cn/windows/win32/api/gdiplusinit/nf-gdiplusinit-gdiplusstartup"/>
    /// </para>
    /// </summary>
    /// <param name="token">
    /// Pointer to a ULONG_PTR that receives a token. Pass the token to <see cref="GdiplusShutdown"/> when you have finished using GDI+.
    /// </param>
    /// <param name="input">
    /// Pointer to a <see cref="GdiplusStartupInput"/>  structure that contains the GDI+ version, a pointer to a debug callback function,
    /// a Boolean value that specifies whether to suppress the background thread,
    /// and a Boolean value that specifies whether to suppress external image codecs.
    /// </param>
    /// <param name="output"></param>
    /// <returns>
    /// If the function succeeds, it returns <see cref="GpStatus.Ok"/>.
    /// If the function fails, it returns one of the other elements of the <see cref="GpStatus"/> enumeration.
    /// </returns>
    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, EntryPoint = "GdiplusStartup", ExactSpelling = true, SetLastError = true)]
    public static extern GpStatus GdiplusStartup([Out] out UIntPtr token, [In] ref GdiplusStartupInput input, [Out] out GdiplusStartupOutput output);

    /// <summary>
    /// <para>
    /// The <see cref="GdiplusStartup"/> function uses the <see cref="GdiplusStartupOutput"/> structure
    /// to return (in its output parameter) a pointer to a hook function and a pointer to an unhook function.
    /// If you set the <see cref="GdiplusStartupInput.SuppressBackgroundThread"/> member of the input parameter to <see langword="true"/>,
    /// then you are responsible for calling those functions to replace the Windows GDI+ background thread.
    /// Call the hook and unhook functions before and after the application's main message loop;
    /// that is, a message loop that is active for the lifetime of GDI+.
    /// Call the hook function before the loop starts, and call the unhook function after the loop ends.
    /// The token parameter of the hook function receives an identifier that you should later pass to the unhook function.
    /// If you do not pass the proper identifier (the one returned by the hook function) to the unhook function,
    /// there will be resource leaks that won't be cleaned up until the process exits.
    /// If you do not want to be responsible for calling the hook and unhook functions,
    /// set the <see cref="GdiplusStartupInput.SuppressBackgroundThread"/> member
    /// of the input parameter (passed to <see cref="GdiplusStartup"/>) to <see langword="false"/>.
    /// </para>
    /// <para>
    /// From: <see href="https://docs.microsoft.com/zh-cn/windows/win32/api/gdiplusinit/ns-gdiplusinit-gdiplusstartupoutput"/>
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct GdiplusStartupOutput
    {
        /// <summary>
        /// Receives a pointer to a hook function.
        /// </summary>
        public IntPtr NotificationHook;

        /// <summary>
        /// Receives a pointer to an unhook function.
        /// </summary>
        public IntPtr NotificationUnhook;
    }

    /// <summary>
    /// <para>
    /// The <see cref="GdiplusStartupInput"/> structure holds a block of arguments that are required by the <see cref="GdiplusStartup"/> function.
    /// </para>
    /// <para>
    /// From: <see href="https://docs.microsoft.com/zh-cn/windows/win32/api/gdiplusinit/ns-gdiplusinit-gdiplusstartupinput"/>
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct GdiplusStartupInput
    {
        /// <summary>
        /// Specifies the version of GDI+. Must be 1.
        /// </summary>
        public uint GdiplusVersion;

        /// <summary>
        /// Pointer to a callback function that GDI+ can call, on debug builds, for assertions and warnings.
        /// The default value is <see cref="IntPtr.Zero"/>.
        /// </summary>
        public IntPtr DebugEventCallback;

        /// <summary>
        /// Boolean value that specifies whether to suppress the GDI+ background thread. 
        /// If you set this member to <see langword="true"/>,
        /// <see cref="GdiplusStartup"/> returns (in its output parameter) a pointer to a hook function and a pointer to an unhook function.
        /// You must call those functions appropriately to replace the background thread.
        /// If you do not want to be responsible for calling the hook and unhook functions,
        /// set this member to <see langword="false"/>. The default value is <see langword="false"/>.
        /// </summary>
        public BOOL SuppressBackgroundThread;

        /// <summary>
        /// Boolean value that specifies whether you want GDI+ to suppress external image codecs.
        /// GDI+ version 1.0 does not support external image codecs, so this parameter is ignored.
        /// </summary>
        public BOOL SuppressExternalCodecs;
    }

    /// <summary>
    /// <para>
    /// The <see cref="GpStatus"/> enumeration indicates the result of a Windows GDI+ method call.
    /// </para>
    /// <para>
    /// From: <see href="https://docs.microsoft.com/zh-cn/windows/win32/api/gdiplustypes/ne-gdiplustypes-status"/>
    /// </para>
    /// </summary>
    public enum GpStatus
    {
        /// <summary>
        /// Indicates that the method call was successful.
        /// </summary>
        Ok,

        /// <summary>
        /// Indicates that there was an error on the method call,
        /// which is identified as something other than those defined by the other elements of this enumeration.
        /// </summary>
        GenericError,

        /// <summary>
        /// Indicates that one of the arguments passed to the method was not valid.
        /// </summary>
        InvalidParameter,

        /// <summary>
        /// Indicates that the operating system is out of memory and could not allocate memory to process the method call.
        /// For an explanation of how constructors use the <see cref="OutOfMemory"/> status, see the Remarks section at the end of this topic.
        /// </summary>
        OutOfMemory,

        /// <summary>
        /// Indicates that one of the arguments specified in the API call is already in use in another thread.
        /// </summary>
        ObjectBusy,

        /// <summary>
        /// Indicates that a buffer specified as an argument in the API call is not large enough to hold the data to be received.
        /// </summary>
        InsufficientBuffer,

        /// <summary>
        /// Indicates that the method is not implemented.
        /// </summary>
        NotImplemented,

        /// <summary>
        /// Indicates that the method generated a Win32 error.
        /// </summary>
        Win32Error,

        /// <summary>
        /// Indicates that the object is in an invalid state to satisfy the API call.
        /// For example, calling Pen::GetColor from a pen that is not a single, solid color results in a <see cref="WrongState"/> status.
        /// </summary>
        WrongState,

        /// <summary>
        /// Indicates that the method was aborted.
        /// </summary>
        Aborted,

        /// <summary>
        /// Indicates that the specified image file or metafile cannot be found.
        /// </summary>
        FileNotFound,

        /// <summary>
        /// Indicates that the method performed an arithmetic operation that produced a numeric overflow.
        /// </summary>
        ValueOverflow,

        /// <summary>
        /// Indicates that a write operation is not allowed on the specified file.
        /// </summary>
        AccessDenied,

        /// <summary>
        /// Indicates that the specified image file format is not known.
        /// </summary>
        UnknownImageFormat,

        /// <summary>
        /// Indicates that the specified font family cannot be found. Either the font family name is incorrect or the font family is not installed.
        /// </summary>
        FontFamilyNotFound,

        /// <summary>
        /// Indicates that the specified style is not available for the specified font family.
        /// </summary>
        FontStyleNotFound,

        /// <summary>
        /// Indicates that the font retrieved from an HDC or LOGFONT is not a TrueType font and cannot be used with GDI+.
        /// </summary>
        NotTrueTypeFont,

        /// <summary>
        /// Indicates that the version of GDI+ that is installed on the system is incompatible with the version with which the application was compiled.
        /// </summary>
        UnsupportedGdiplusVersion,

        /// <summary>
        /// Indicates that the GDI+API is not in an initialized state.
        /// To function, all GDI+ objects require that GDI+ be in an initialized state.
        /// Initialize GDI+ by calling GdiplusStartup.
        /// </summary>
        GdiplusNotInitialized,

        /// <summary>
        /// Indicates that the specified property does not exist in the image.
        /// </summary>
        PropertyNotFound,

        /// <summary>
        /// Indicates that the specified property is not supported by the format of the image and, therefore, cannot be set.
        /// </summary>
        PropertyNotSupported,

        /// <summary>
        /// Indicates that the color profile required to save an image in CMYK format was not found.
        /// </summary>
        ProfileNotFound,
    }

    /// <summary>
    /// Creates an Image object based on a file. This flat function does not use ICM.
    /// <param name="filename">Image file name.</param>
    /// <param name="image">Image object.</param>
    /// <returns>
    /// </returns>
    /// </summary>
    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, EntryPoint = "GdipLoadImageFromFile", ExactSpelling = true, SetLastError = true)]
    public static extern GpStatus GdipLoadImageFromFile([In][MarshalAs(UnmanagedType.LPWStr)] string filename, [Out] out IntPtr image);

    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, EntryPoint = "GdipLoadImageFromStream", ExactSpelling = true, SetLastError = true)]
    public static extern GpStatus GdipLoadImageFromStream([In] IStream stream, [Out] out IntPtr image);
    // System.Runtime.InteropServices.ComTypes.
    /// <summary>
    /// Creates a Graphics object that is associated with a specified device context.
    /// </summary>
    /// <param name="hdc">Device context.</param>
    /// <param name="graphics">Graphics object.</param>
    /// <returns></returns>
    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, EntryPoint = "GdipCreateFromHDC", ExactSpelling = true, SetLastError = true)]
    public static extern GpStatus GdipCreateFromHDC([In] HDC hdc, [Out] out IntPtr graphics);

    /// <summary>
    /// Draws an image.
    /// </summary>
    /// <param name="graphics">Graphics object.</param>
    /// <param name="image">Image object.</param>
    /// <param name="x">X postion.</param>
    /// <param name="y">Y position.</param>
    /// <param name="height">Height.</param>
    /// <param name="width">Width.</param>
    /// <returns></returns>
    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, EntryPoint = "GdipDrawImageRectI", ExactSpelling = true, SetLastError = true)]
    public static extern GpStatus GdipDrawImageRectI([In] IntPtr graphics, [In] IntPtr image, [In] int x, [In] int y, [In] int width, [In] int height);

    /// <summary>
    /// <para>
    /// The <see cref="GdiplusShutdown"/> function cleans up resources used by Windows GDI+.
    /// Each call to <see cref="GdiplusStartup"/> should be paired with a call to <see cref="GdiplusShutdown"/>.
    /// </para>
    /// <para>
    /// From: <see href="https://docs.microsoft.com/zh-cn/windows/win32/api/gdiplusinit/nf-gdiplusinit-gdiplusshutdown"/>
    /// </para>
    /// </summary>
    /// <param name="token">
    /// Token returned by a previous call to <see cref="GdiplusStartup"/>.
    /// </param>
    [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, EntryPoint = "GdiplusShutdown", ExactSpelling = true, SetLastError = true)]
    public static extern void GdiplusShutdown([In] UIntPtr token);

    // ----------- Gdi32 Struct ----------

    /// <summary>
    /// <para>
    /// The <see cref="BLENDFUNCTION"/> structure controls blending by specifying the blending functions for source and destination bitmaps.
    /// </para>
    /// <para>
    /// From: <see href="https://docs.microsoft.com/zh-cn/windows/win32/api/wingdi/ns-wingdi-blendfunction"/>
    /// </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct BLENDFUNCTION
    {
        /// <summary>
        /// AC_SRC_OVER
        /// </summary>
        public static readonly byte AC_SRC_OVER = 0;

        /// <summary>
        /// This flag is set when the bitmap has an Alpha channel (that is, per-pixel alpha)
        /// Note that the APIs use premultiplied alpha, which means that the red, green and blue channel values in the bitmap
        /// must be premultiplied with the alpha channel value. 
        /// For example, if the alpha channel value is x, the red, green and blue channels must be multiplied by x and
        /// divided by 0xff prior to the call.
        /// </summary>
        public static readonly byte AC_SRC_ALPHA = 1;


        /// <summary>
        /// The source blend operation. 
        /// Currently, the only source and destination blend operation that has been defined is <see cref="AC_SRC_OVER"/>.
        /// For details, see the following Remarks section.
        /// </summary>
        public byte BlendOp;

        /// <summary>
        /// Must be zero.
        /// </summary>
        public byte BlendFlags;

        /// <summary>
        /// Specifies an alpha transparency value to be used on the entire source bitmap.
        /// The <see cref="SourceConstantAlpha"/> value is combined with any per-pixel alpha values in the source bitmap.
        /// If you set <see cref="SourceConstantAlpha"/> to 0, it is assumed that your image is transparent. 
        /// Set the <see cref="SourceConstantAlpha"/> value to 255 (opaque) when you only want to use per-pixel alpha values.
        /// </summary>
        public byte SourceConstantAlpha;

        /// <summary>
        /// This member controls the way the source and destination bitmaps are interpreted.
        /// <see cref="AlphaFormat"/> has value of <see cref="AC_SRC_ALPHA"/>
        /// </summary>
        public byte AlphaFormat;
    }
}
