using System.Collections.ObjectModel;
using Ra2ImageTool.Models;

namespace Ra2ImageTool.Data
{
    public class GData
    {
        internal const double VERSION = 1.6;

        internal static int[,] BayerMatrix = {
            {  0, 32,  8, 40,  2, 34, 10, 42 },
            { 48, 16, 56, 24, 50, 18, 58, 26 },
            { 12, 44,  4, 36, 14, 46,  6, 38 },
            { 60, 28, 52, 20, 62, 30, 54, 22 },
            {  3, 35, 11, 43,  1, 33,  9, 41 },
            { 51, 19, 59, 27, 49, 17, 57, 25 },
            { 15, 47,  7, 39, 13, 45,  5, 37 },
            { 63, 31, 55, 23, 61, 29, 53, 21 }
        };

        internal const int MatrixSize = 8;

        internal static readonly object Locker_changeAllImg = new object();

        public enum OverlayMode
        {
            叠加在上,
            叠加在下
        }

        public enum CreatePalMode
        {
            主要颜色,
            主要颜色补小像素,
            OctreeQuantizer
        }

        public enum ListName
        {
            全部,
            操作,
            叠加,
            空
        }

        internal const string ImageFileNotExists = "图片文件不存在";

        internal static ObservableCollection<ImageInfoModel> ListViewData = new ObservableCollection<ImageInfoModel>();

        public static UIDataModel UIData;

        internal static PaletteConfigModel PaletteConfig = new PaletteConfigModel();
    }
}
