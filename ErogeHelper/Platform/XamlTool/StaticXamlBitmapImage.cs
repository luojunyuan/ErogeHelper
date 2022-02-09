using System.Drawing;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ErogeHelper.Platform.XamlTool;

public static class StaticXamlBitmapImage
{
    public static readonly BitmapImage TransparentImage =
        LoadBitmapFromResource("assets/text_background/transparent.png");
    public static readonly BitmapImage AquaGreenImage =
        LoadBitmapFromResource("assets/text_background/aqua_green.png");
    public static readonly BitmapImage GreenImage =
        LoadBitmapFromResource("assets/text_background/green.png");
    public static readonly BitmapImage PinkImage =
        LoadBitmapFromResource("assets/text_background/pink.png");

    /// <summary>
    /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 
    /// 'Embedded resource'.
    /// </summary>
    /// <param name="pathInApplication">Path without starting slash</param>
    /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentioned, I will use the calling
    /// assembly</param>
    /// <returns></returns>
    public static BitmapImage LoadBitmapFromResource(string pathInApplication, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        if (pathInApplication[0] == '/')
            pathInApplication = pathInApplication[1..];

        try
        {
            var uri = new Uri(
                @"pack://application:,,,/" + assembly.GetName().Name + ";component/" + pathInApplication,
                UriKind.Absolute);
            return new BitmapImage(uri);
        }
        catch (UriFormatException)
        {
            // Running in unit test
            return new BitmapImage();
        }
    }

    private static readonly int LightGreen = Color.LightGreen.ToArgb();
    private static readonly int Green = Color.Green.ToArgb();
    private static readonly int Pink = Color.Pink.ToArgb();
    // private static readonly int Transparent = Color.Transparent.ToArgb();

    public static BitmapImage ToBitmapImage(this Color color)
    {
        var argbValue = color.ToArgb();
        return argbValue == LightGreen ? AquaGreenImage :
               argbValue == Green ? GreenImage :
               argbValue == Pink ? PinkImage :
               TransparentImage;
    }
}
