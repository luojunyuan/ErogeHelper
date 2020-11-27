using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ErogeHelper_Core.Common
{
    static class Utils
    {
        public static BitmapImage PEIcon2BitmapImage(string fullPath)
        {
            BitmapImage result = new BitmapImage();
            Stream stream = new MemoryStream();

            var iconBitmap = Icon.ExtractAssociatedIcon(fullPath).ToBitmap();
            iconBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            iconBitmap.Dispose();   
            result.BeginInit();
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();

            return result;
        }

        /// <summary>
        /// Load a resource WPF-BitmapImage (png, bmp, ...) from embedded resource defined as 'Resource' not as 'Embedded resource'.
        /// </summary>
        /// <param name="pathInApplication">Path without starting slash</param>
        /// <param name="assembly">Usually 'Assembly.GetExecutingAssembly()'. If not mentionned, I will use the calling assembly</param>
        /// <returns></returns>
        public static BitmapImage LoadBitmapFromResource(string pathInApplication, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            if (pathInApplication[0] == '/')
            {
                pathInApplication = pathInApplication.Substring(1);
            }
            return new BitmapImage(new Uri(@"pack://application:,,,/" + assembly.GetName().Name + ";component/" + pathInApplication, UriKind.Absolute));
        }
    }
}
