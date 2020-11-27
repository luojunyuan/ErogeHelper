using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper_Core.Common.Extension
{
    static class ProcessExtention
    {
        public static string GetMainModuleFileName(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return NativeMethods.QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength) != 0 ?
                fileNameBuilder.ToString() :
                "";
        }
    }
}
