using System;

namespace Ra2ImageTool.Funcs
{
    internal class GetPath
    {
        internal static string RunPath = AppDomain.CurrentDomain.BaseDirectory;

        internal static string GetExportImagePath()
        {
            return $@"{RunPath}输出图像";
        }

        internal static string GetExportPalPath()
        {
            return $@"{RunPath}输出色盘";
        }

        internal static string GetGifToPngPath()
        {
            return $@"{RunPath}GIF转PNG";
        }
    }
}
