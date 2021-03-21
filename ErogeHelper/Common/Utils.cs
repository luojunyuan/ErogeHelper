using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace ErogeHelper.Common
{
    public static class Utils
    {
        public static BitmapImage PeIcon2BitmapImage(string fullPath)
        {
            BitmapImage result = new ();
            Stream stream = new MemoryStream();

            var iconBitmap = Icon.ExtractAssociatedIcon(fullPath)!.ToBitmap();
            iconBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            iconBitmap.Dispose();
            result.BeginInit();
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();

            return result;
        }
    }
}