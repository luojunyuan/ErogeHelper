﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ErogeHelper.AssistiveTouch.NativeMethods
{
    internal class Kernel32
    {
        private const string Kernel32Dll = "kernel32.dll";

        [DllImport(Kernel32Dll, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle([Optional] string lpModuleName);
    }
}