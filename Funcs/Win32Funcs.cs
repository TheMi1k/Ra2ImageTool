﻿using System;
using System.Runtime.InteropServices;

namespace Ra2ImageTool.Funcs
{
    internal class Win32Funcs
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}
