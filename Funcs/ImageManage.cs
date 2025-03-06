using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using Ra2ImageTool.Data;
using Color = System.Drawing.Color;
using System.IO;
using System.Drawing.Imaging;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Imaging.Quantizers;
using ImageProcessor;

namespace Ra2ImageTool.Funcs
{
    internal class ImageManage
    {
        //internal static void ConvertImage(int index)
        //{
        //    if (GData.ListViewData.Count == 0)
        //    {
        //        return;
        //    }
        //    if (index > GData.ListViewData.Count - 1)
        //    {
        //        return;
        //    }
        //    if (GData.ListViewData[index].InImg == null)
        //    {
        //        return;
        //    }

        //    GData.ListViewData[index].IsChanged = true;

        //    int alpha = GData.ListViewData[index].Alpha;
        //    double transparentDiffusion = GData.ListViewData[index].TransparentDiffusion;
        //    double lightness = GData.ListViewData[index].Lightness;
        //    bool isTransparent = GData.ListViewData[index].IsTransparent;

        //    double outlineTransparentStep = 0;

        //    if (isTransparent)
        //    {
        //        if (GData.ListViewData[index].DistanceMatrix == null)
        //        {
        //            GData.ListViewData[index].DistanceMatrix = GetDistanceMatrix(GData.ListViewData[index].InImg);
        //        }

        //        int rows = GData.ListViewData[index].DistanceMatrix.GetLength(0);
        //        int cols = GData.ListViewData[index].DistanceMatrix.GetLength(1);

        //        double maxDistance = -1;
        //        for (int r = 0; r < rows; r++)
        //        {
        //            for (int c = 0; c < cols; c++)
        //            {
        //                if (GData.ListViewData[index].DistanceMatrix[r, c] > maxDistance)
        //                {
        //                    maxDistance = GData.ListViewData[index].DistanceMatrix[r, c];
        //                }
        //            }
        //        }

        //        outlineTransparentStep = 255 / maxDistance;
        //    }

        //    for (int y = 0; y < GData.ListViewData[index].InImg.Height; y++)
        //    {
        //        for (int x = 0; x < GData.ListViewData[index].InImg.Width; x++)
        //        {
        //            int newAlpha = -1;
        //            Color pixel = GData.ListViewData[index].InImg.GetPixel(x, y);
        //            int _alpha = pixel.A;

        //            if (isTransparent && pixel.A > 0)
        //            {
        //                _alpha = (int)Math.Round((GData.ListViewData[index].DistanceMatrix[y, x] * outlineTransparentStep));
        //                if (_alpha > 255)
        //                {
        //                    _alpha = 255;
        //                }
        //                if (_alpha < 0)
        //                {
        //                    _alpha = 0;
        //                }

        //                newAlpha = _alpha;

        //                if (pixel.A > 0)
        //                {
        //                    _alpha -= alpha;
        //                }
        //            }
        //            else
        //            {
        //                if (pixel.A > 0)
        //                {
        //                    _alpha = pixel.A - alpha;
        //                }
        //            }

        //            if (_alpha > 255)
        //            {
        //                _alpha = 255;
        //            }
        //            if (_alpha < 0)
        //            {
        //                _alpha = 0;
        //            }

        //            if (_alpha >= 255)
        //            {
        //                GData.ListViewData[index].OutImg.SetPixel(x, y, SetPixelLightness(pixel, lightness));
        //            }
        //            else if (_alpha <= 0)
        //            {
        //                GData.ListViewData[index].OutImg.SetPixel(x, y, Color.Transparent);
        //            }
        //            else
        //            {
        //                int matrixValue = GData.BayerMatrix[y % GData.MatrixSize, x % GData.MatrixSize];
        //                if (_alpha * transparentDiffusion > matrixValue)
        //                {
        //                    GData.ListViewData[index].OutImg.SetPixel(x, y, SetPixelLightness(pixel, lightness, newAlpha));
        //                }
        //                else
        //                {
        //                    GData.ListViewData[index].OutImg.SetPixel(x, y, Color.Transparent);
        //                }
        //            }
        //        }
        //    }

        //    GData.ListViewData[index].DistanceMatrix = null;

        //    GC.Collect();
        //}

        internal static void ConvertImage(int index)
        {
            if (GData.ListViewData.Count == 0)
            {
                return;
            }
            if (index > GData.ListViewData.Count - 1)
            {
                return;
            }
            if (GData.ListViewData[index].InImg == null)
            {
                return;
            }

            GData.ListViewData[index].IsChanged = true;

            int alpha = GData.ListViewData[index].Alpha;
            double transparentDiffusion = GData.ListViewData[index].TransparentDiffusion;
            double lightness = GData.ListViewData[index].Lightness;
            bool isTransparent = GData.ListViewData[index].IsTransparent;

            double outlineTransparentStep = 0;

            if (isTransparent)
            {
                if (GData.ListViewData[index].DistanceMatrix == null)
                {
                    GData.ListViewData[index].DistanceMatrix = GetDistanceMatrix(GData.ListViewData[index].InImg);
                }

                int rows = GData.ListViewData[index].DistanceMatrix.GetLength(0);
                int cols = GData.ListViewData[index].DistanceMatrix.GetLength(1);

                double maxDistance = -1;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (GData.ListViewData[index].DistanceMatrix[r, c] > maxDistance)
                        {
                            maxDistance = GData.ListViewData[index].DistanceMatrix[r, c];
                        }
                    }
                }

                outlineTransparentStep = 255 / maxDistance;
            }

            Rectangle rect = new Rectangle(0, 0, GData.ListViewData[index].InImg.Width, GData.ListViewData[index].InImg.Height);
            BitmapData bmpData = GData.ListViewData[index].InImg.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * GData.ListViewData[index].InImg.Height;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            GData.ListViewData[index].InImg.UnlockBits(bmpData);

            for (int y = 0; y < GData.ListViewData[index].InImg.Height; y++)
            {
                for (int x = 0; x < GData.ListViewData[index].InImg.Width; x++)
                {
                    int newAlpha = -1;

                    int stride = Math.Abs(bmpData.Stride);
                    int imgByteIndex = (y * stride) + (x * 4);

                    byte pixelB = rgbValues[imgByteIndex];
                    byte pixelG = rgbValues[imgByteIndex + 1];
                    byte pixelR = rgbValues[imgByteIndex + 2];
                    byte pixelA = rgbValues[imgByteIndex + 3];

                    int _alpha = pixelA;

                    if (isTransparent && pixelA > 0)
                    {
                        _alpha = (int)Math.Round((GData.ListViewData[index].DistanceMatrix[y, x] * outlineTransparentStep));
                        if (_alpha > 255)
                        {
                            _alpha = 255;
                        }
                        if (_alpha < 0)
                        {
                            _alpha = 0;
                        }

                        newAlpha = _alpha;

                        if (pixelA > 0)
                        {
                            _alpha -= alpha;
                        }
                    }
                    else
                    {
                        if (pixelA > 0)
                        {
                            _alpha = pixelA - alpha;
                        }
                    }

                    if (_alpha > 255)
                    {
                        _alpha = 255;
                    }
                    if (_alpha < 0)
                    {
                        _alpha = 0;
                    }

                    if (_alpha >= 255)
                    {
                        byte[] colors = SetPixelLightness(pixelA, pixelR, pixelG, pixelB, lightness);

                        rgbValues[imgByteIndex] = colors[3]; // B
                        rgbValues[imgByteIndex + 1] = colors[2]; // G
                        rgbValues[imgByteIndex + 2] = colors[1]; // R
                        rgbValues[imgByteIndex + 3] = colors[0]; // A
                    }
                    else if (_alpha <= 0)
                    {
                        rgbValues[imgByteIndex] = 255; // B
                        rgbValues[imgByteIndex + 1] = 255; // G
                        rgbValues[imgByteIndex + 2] = 255; // R
                        rgbValues[imgByteIndex + 3] = 0; // A
                    }
                    else
                    {
                        int matrixValue = GData.BayerMatrix[y % GData.MatrixSize, x % GData.MatrixSize];
                        if (_alpha * transparentDiffusion > matrixValue)
                        {
                            byte[] colors = SetPixelLightness(pixelA, pixelR, pixelG, pixelB, lightness, newAlpha);
                            
                            rgbValues[imgByteIndex] = colors[3]; // B
                            rgbValues[imgByteIndex + 1] = colors[2]; // G
                            rgbValues[imgByteIndex + 2] = colors[1]; // R
                            rgbValues[imgByteIndex + 3] = colors[0]; // A
                        }
                        else
                        {
                            rgbValues[imgByteIndex] = 255; // B
                            rgbValues[imgByteIndex + 1] = 255; // G
                            rgbValues[imgByteIndex + 2] = 255; // R
                            rgbValues[imgByteIndex + 3] = 0; // A
                        }
                    }
                }
            }

            Rectangle rectOut = new Rectangle(0, 0, GData.ListViewData[index].OutImg.Width, GData.ListViewData[index].OutImg.Height);
            BitmapData bmpDataOut = GData.ListViewData[index].OutImg.LockBits(rectOut, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            IntPtr ptrOut = bmpDataOut.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptrOut, rgbValues.Length);
            GData.ListViewData[index].OutImg.UnlockBits(bmpData);

            GData.ListViewData[index].DistanceMatrix = null;

            GC.Collect();
        }

        private static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        //private static Color SetPixelLightness(Color color, double k, int newAlpha = -1)
        //{
        //    int A;
        //    if (newAlpha != -1)
        //    {
        //        A = newAlpha;
        //    }
        //    else
        //    {
        //        A = color.A;
        //    }

        //    float alphaFactor = A / 255.0f;

        //    int R, G, B;

        //    if (k < 0)
        //    {
        //        // 变黑 (k 负)
        //        float darkFactor = (float)Math.Pow(alphaFactor, Math.Abs(k));
        //        R = (int)(color.R * darkFactor);
        //        G = (int)(color.G * darkFactor);
        //        B = (int)(color.B * darkFactor);
        //    }
        //    else
        //    {
        //        // 变白 (k 正)
        //        R = (int)(color.R + (255 - color.R) * (1 - alphaFactor) * k);
        //        G = (int)(color.G + (255 - color.G) * (1 - alphaFactor) * k);
        //        B = (int)(color.B + (255 - color.B) * (1 - alphaFactor) * k);
        //    }

        //    return Color.FromArgb(255, Clamp(R), Clamp(G), Clamp(B));
        //}

        private static byte[] SetPixelLightness(byte pixelA, byte pixelR, byte pixelG, byte pixelB, double k, int newAlpha = -1)
        {
            byte[] result = new byte[4];

            int A;
            if (newAlpha != -1)
            {
                A = newAlpha;
            }
            else
            {
                A = pixelA;
            }

            float alphaFactor = A / 255.0f;

            int R, G, B;

            if (k < 0)
            {
                // 变黑 (k 负)
                float darkFactor = (float)Math.Pow(alphaFactor, Math.Abs(k));
                R = (int)(pixelR * darkFactor);
                G = (int)(pixelG * darkFactor);
                B = (int)(pixelB * darkFactor);
            }
            else
            {
                // 变白 (k 正)
                R = (int)(pixelR + (255 - pixelR) * (1 - alphaFactor) * k);
                G = (int)(pixelG + (255 - pixelG) * (1 - alphaFactor) * k);
                B = (int)(pixelB + (255 - pixelB) * (1 - alphaFactor) * k);
            }

            result[0] = 255;
            result[1] = (byte)Clamp(R);
            result[2] = (byte)Clamp(G);
            result[3] = (byte)Clamp(B);

            return result;
        }

        internal static ImageSource ToImageSource(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                return null;
            }

            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            if (!Win32Funcs.DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }

        internal static List<Bitmap> ExtractGifFrames(string gifPath)
        {
            List<Bitmap> frames = new List<Bitmap>();

            using (FileStream fs = new FileStream(gifPath, FileMode.Open, FileAccess.Read))
            {
                var gifDecoder = new GifBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                foreach (var frame in gifDecoder.Frames)
                {
                    frames.Add(BitmapSourceToBitmap(frame));
                }
            }

            return frames;
        }

        internal static void DisposeBitmap(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                try
                {
                    bitmap.Dispose();

                    //if (!Win32Funcs.DeleteObject(bitmap.GetHbitmap()))
                    //{
                    //    throw new Win32Exception();
                    //}
                }
                catch
                {

                }
            }
        }

        internal static double[,] GetDistanceMatrix(Bitmap bitmap)
        {
            byte[,] matrix = new byte[bitmap.Height, bitmap.Width];

            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            int xCount = 0;
            int yCount = 0;
            int pixelSize = 4;
            for (int i = 0; i < rgbValues.Length; i += pixelSize)
            {
                //byte blue = rgbValues[i];
                //byte green = rgbValues[i + 1];
                //byte red = rgbValues[i + 2];
                byte alpha = rgbValues[i + 3];

                //Color c = Color.FromArgb(255, (int)Math.Floor(red / 4f), (int)Math.Floor(green / 4f), (int)Math.Floor(blue / 4f));

                if (alpha == 0)
                {
                    matrix[yCount, xCount] = 0;
                }
                else
                {
                    matrix[yCount, xCount] = 1;
                }

                xCount++;
                if (xCount >= bitmap.Width)
                {
                    xCount = 0;
                    yCount++;
                }
            }

            bitmap.UnlockBits(bmpData);


            //for (int h = 0; h < bitmap.Height; h++)
            //{
            //    for (int w = 0; w < bitmap.Width; w++)
            //    {
            //        var c = bitmap.GetPixel(w, h);
            //        if (c.A == 0)
            //        {
            //            matrix[h, w] = 0;
            //        }
            //        else
            //        {
            //            matrix[h, w] = 1;
            //        }
            //    }
            //}

            double[,] distanceMatrix = GetDistance(matrix);

            return distanceMatrix;
        }

        private static double[,] GetDistance(byte[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] dist = new double[rows, cols];
            Queue<(int, int)> queue = new Queue<(int, int)>();

            (int dx, int dy, double cost)[] directions = {
                (-1, 0, 1), (1, 0, 1), (0, -1, 1), (0, 1, 1), // 上下左右
                (-1, -1, 1.4141), (-1, 1, 1.4141), (1, -1, 1.4141), (1, 1, 1.4141)  // 对角线
            };

            // 初始化距离矩阵
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (matrix[r, c] == 0)
                    {
                        dist[r, c] = 0;
                        queue.Enqueue((r, c));
                    }
                    else
                    {
                        dist[r, c] = double.MaxValue;
                    }
                }
            }

            // BFS 计算距离
            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();

                foreach (var (dx, dy, cost) in directions)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && ny >= 0 && nx < rows && ny < cols)
                    {
                        double newDistance = dist[x, y] + cost;
                        if (newDistance < dist[nx, ny])
                        {
                            dist[nx, ny] = newDistance;
                            queue.Enqueue((nx, ny));
                        }
                    }
                }
            }

            return dist;
        }

        internal static Bitmap MergeBitmaps(Bitmap outImg, Bitmap overlayImg, int overlayXOffset, int overlayYOffset, GData.OverlayMode mode)
        {
            if (outImg == null && overlayImg == null)
            {
                return null;
            }
            if (overlayImg == null)
            {
                return new Bitmap(outImg);
            }

            int width;
            int height;

            if (outImg == null)
            {
                width = overlayImg.Width + overlayXOffset;
                height = overlayImg.Height + overlayYOffset;
            }
            else
            {
                width = outImg.Width;
                height = outImg.Height;
            }

            BitmapSource outImgBitmapSource = null;
            if (outImg != null)
            {
                outImgBitmapSource = BitmapToBitmapSource(outImg);
            }

            BitmapSource overlayImgBitmapSource = BitmapToBitmapSource(overlayImg);

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                if (outImgBitmapSource == null)
                {
                    dc.DrawImage(overlayImgBitmapSource, new Rect(overlayXOffset, overlayYOffset, overlayImg.Width, overlayImg.Height));
                }
                else if (mode == GData.OverlayMode.叠加在上)
                {
                    // 下层
                    dc.DrawImage(outImgBitmapSource, new Rect(0, 0, outImg.Width, outImg.Height));

                    // 上层
                    dc.DrawImage(overlayImgBitmapSource, new Rect(overlayXOffset, overlayYOffset, overlayImg.Width, overlayImg.Height));
                }
                else
                {
                    // 下层
                    dc.DrawImage(overlayImgBitmapSource, new Rect(overlayXOffset, overlayYOffset, overlayImg.Width, overlayImg.Height));

                    // 上层
                    dc.DrawImage(outImgBitmapSource, new Rect(0, 0, outImg.Width, outImg.Height));
                }
            }

            RenderTargetBitmap mergedBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            mergedBitmap.Render(visual);

            return BitmapSourceToBitmap(mergedBitmap);
        }

        private static Bitmap BitmapSourceToBitmap(BitmapSource bitmapSource)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);

                return new Bitmap(stream);
            }
        }

        private static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapDecoder decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return decoder.Frames[0];
            }
        }

        /// <summary>
        /// 移除相似颜色
        /// </summary>
        /// <param name="colorDic"></param>
        private static void RemoveSimilarColors(Dictionary<Color, int> colorDic)
        {
            List<Color> allColorList = new List<Color>();
            HashSet<Color> removeList = new HashSet<Color>();

            foreach (var kv in colorDic)
            {
                allColorList.Add(kv.Key);
            }

            for (int i = 0; i < allColorList.Count; i++)
            {
                for (int j = 0; j < allColorList.Count; j++)
                {
                    if (i != j && !removeList.Contains(allColorList[i]))
                    {
                        if (Math.Abs(allColorList[i].R - allColorList[j].R) + Math.Abs(allColorList[i].G - allColorList[j].G) + Math.Abs(allColorList[i].B - allColorList[j].B) <= 1)
                        {
                            if (colorDic[allColorList[i]] > colorDic[allColorList[j]])
                            {
                                removeList.Add(allColorList[j]);
                            }
                        }
                    }
                }
            }

            foreach (var c in removeList)
            {
                colorDic.Remove(c);
            }
        }

        internal static async Task<List<Color>> CreatePalette(int palColorNum, Color paletteBackground, string mode)
        {
            if (mode == GData.CreatePalMode.主要颜色补小像素.ToString())
            {
                if (palColorNum < 150)
                {
                    throw new Exception("该色盘生成方法颜色数量不能小于150");
                }
                return await ImageManage.Generate256ColorPalette_Mode3(palColorNum, paletteBackground);
            }
            else if (mode == GData.CreatePalMode.主要颜色.ToString())
            {
                return await Generate256ColorPalette_Mode1(palColorNum, paletteBackground);
            }
            else
            {
                return await Generate256ColorPalette_Mode2(palColorNum, paletteBackground);
            }
        }

        /// <summary>
        /// 生成色盘(主要颜色优先)
        /// </summary>
        /// <param name="palColorNum"></param>
        /// <returns></returns>
        internal static async Task<List<Color>> Generate256ColorPalette_Mode1(int palColorNum, Color paletteBackground)
        {
            Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

            int sucCount = 0;
            int maxCount = GData.ListViewData.Count;

            GData.UIData.SetProgressUI(sucCount, maxCount);

            await Task.Run(() =>
            {
                foreach (var item in GData.ListViewData)
                {
                    Bitmap resultImg = MergeBitmaps(item.OutImg, item.ImgOverlay, item.OverlayXOffset, item.OverlayYOffset, item.OverlayMode);

                    if (resultImg != null)
                    {
                        Rectangle rect = new Rectangle(0, 0, resultImg.Width, resultImg.Height);
                        BitmapData bmpData = resultImg.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        IntPtr ptr = bmpData.Scan0;
                        int bytes = Math.Abs(bmpData.Stride) * resultImg.Height;
                        byte[] rgbValues = new byte[bytes];
                        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                        int pixelSize = 4;
                        for (int i = 0; i < rgbValues.Length; i += pixelSize)
                        {
                            byte blue = rgbValues[i];
                            byte green = rgbValues[i + 1];
                            byte red = rgbValues[i + 2];
                            byte alpha = rgbValues[i + 3];

                            if (alpha == 0)
                            {
                                continue;
                            }

                            Color c = Color.FromArgb(255, (int)Math.Floor(red / 4f), (int)Math.Floor(green / 4f), (int)Math.Floor(blue / 4f));

                            if (colorCounts.ContainsKey(c))
                            {
                                colorCounts[c]++;
                            }
                            else
                            {
                                colorCounts[c] = 1;
                            }
                        }

                        resultImg.UnlockBits(bmpData);

                        DisposeBitmap(resultImg);
                    }

                    sucCount += 1;

                    GData.UIData.SetProgressUI(sucCount, maxCount);
                }
            });

            if (colorCounts.Count > palColorNum - 1)
            {
                RemoveSimilarColors(colorCounts);
            }

            var result = colorCounts.OrderByDescending(c => c.Value)
                              .Take(palColorNum - 1)
                              .Select(c => c.Key)
                              .ToList();


            Color backgroundColor = Color.FromArgb(255, (int)Math.Floor(paletteBackground.R / 4f), (int)Math.Floor(paletteBackground.G / 4f), (int)Math.Floor(paletteBackground.B / 4f));

            result.Insert(0, backgroundColor);

            while (true)
            {
                if (result.Count >= 256)
                {
                    break;
                }

                result.Add(backgroundColor);
            }

            GC.Collect();

            return result;
        }

        /// <summary>
        /// 生成色盘(OctreeQuantizer)
        /// </summary>
        /// <param name="palColorNum"></param>
        /// <returns></returns>
        internal static async Task<List<Color>> Generate256ColorPalette_Mode2(int palColorNum, Color paletteBackground)
        {
            HashSet<Color> hs = new HashSet<Color>();

            int sucCount = 0;
            int maxCount = GData.ListViewData.Count;

            GData.UIData.SetProgressUI(sucCount, maxCount);

            await Task.Run(() =>
            {
                foreach (var item in GData.ListViewData)
                {
                    Bitmap resultImg = MergeBitmaps(item.OutImg, item.ImgOverlay, item.OverlayXOffset, item.OverlayYOffset, item.OverlayMode);

                    if (resultImg != null)
                    {
                        Rectangle rect = new Rectangle(0, 0, resultImg.Width, resultImg.Height);
                        BitmapData bmpData = resultImg.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        IntPtr ptr = bmpData.Scan0;
                        int bytes = Math.Abs(bmpData.Stride) * resultImg.Height;
                        byte[] rgbValues = new byte[bytes];
                        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                        int pixelSize = 4;
                        for (int i = 0; i < rgbValues.Length; i += pixelSize)
                        {
                            byte blue = rgbValues[i];
                            byte green = rgbValues[i + 1];
                            byte red = rgbValues[i + 2];
                            byte alpha = rgbValues[i + 3];

                            if (alpha == 0)
                            {
                                continue;
                            }

                            Color c = Color.FromArgb(255, red, green, blue);

                            hs.Add(c);
                        }

                        resultImg.UnlockBits(bmpData);

                        DisposeBitmap(resultImg);
                    }

                    sucCount += 1;

                    GData.UIData.SetProgressUI(sucCount, maxCount);
                }
            });

            Bitmap allColorBitmap = new Bitmap(hs.Count, 1);
            int count = 0;
            foreach (var c in hs)
            {
                allColorBitmap.SetPixel(count, 0, c);
                count++;
            }

            ImageFactory factory = new ImageFactory();
            factory.Load(allColorBitmap);
            ISupportedImageFormat format = new PngFormat { Quality = 100, IsIndexed = true, Quantizer = new OctreeQuantizer(palColorNum - 1, 8) };
            factory.Format(format);
            MemoryStream stream = new MemoryStream();
            factory.Save(stream);
            Bitmap src = new Bitmap(stream);
            stream.Dispose();
            DisposeBitmap(allColorBitmap);

            HashSet<Color> set = new HashSet<Color>();

            for (int x = 0; x < src.Width; x++)
            {
                var c = src.GetPixel(x, 0);
                set.Add(Color.FromArgb(255, c.R / 4, c.G / 4, c.B / 4));
            }

            DisposeBitmap(src);

            var result = set.ToList();

            Color addColor = Color.FromArgb(255, (int)Math.Floor(paletteBackground.R / 4f), (int)Math.Floor(paletteBackground.G / 4f), (int)Math.Floor(paletteBackground.B / 4f));

            result.Insert(0, addColor);

            while (true)
            {
                if (result.Count >= 256)
                {
                    break;
                }

                result.Add(addColor);
            }

            GC.Collect();

            return result;
        }

        /// <summary>
        /// 生成色盘(补全少部分像素细节)
        /// </summary>
        /// <param name="palColorNum"></param>
        /// <returns></returns>
        internal static async Task<List<Color>> Generate256ColorPalette_Mode3(int palColorNum, Color paletteBackground)
        {
            Dictionary<Color, int> colorCounts = new Dictionary<Color, int>();

            int sucCount = 0;
            int maxCount = GData.ListViewData.Count;

            GData.UIData.SetProgressUI(sucCount, maxCount);

            List<Color> colorBase = new List<Color>();
            for (int r = 4; r <= 244; r += 60)
            {
                for (int g = 4; g <= 244; g += 60)
                {
                    for (int b = 4; b <= 244; b += 60)
                    {
                        colorBase.Add(Color.FromArgb(255, r / 4, g / 4, b / 4));
                    }
                }
            }

            await Task.Run(() =>
            {
                foreach (var item in GData.ListViewData)
                {
                    Bitmap resultImg = MergeBitmaps(item.OutImg, item.ImgOverlay, item.OverlayXOffset, item.OverlayYOffset, item.OverlayMode);

                    if (resultImg != null)
                    {
                        Rectangle rect = new Rectangle(0, 0, resultImg.Width, resultImg.Height);
                        BitmapData bmpData = resultImg.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        IntPtr ptr = bmpData.Scan0;
                        int bytes = Math.Abs(bmpData.Stride) * resultImg.Height;
                        byte[] rgbValues = new byte[bytes];
                        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                        int pixelSize = 4;
                        for (int i = 0; i < rgbValues.Length; i += pixelSize)
                        {
                            byte blue = rgbValues[i];
                            byte green = rgbValues[i + 1];
                            byte red = rgbValues[i + 2];
                            byte alpha = rgbValues[i + 3];

                            if (alpha == 0)
                            {
                                continue;
                            }

                            Color c = Color.FromArgb(255, (int)Math.Floor(red / 4f), (int)Math.Floor(green / 4f), (int)Math.Floor(blue / 4f));

                            if (colorCounts.ContainsKey(c))
                            {
                                colorCounts[c]++;
                            }
                            else
                            {
                                colorCounts[c] = 1;
                            }
                        }

                        resultImg.UnlockBits(bmpData);

                        DisposeBitmap(resultImg);
                    }

                    sucCount += 1;

                    GData.UIData.SetProgressUI(sucCount, maxCount);
                }

            });

            HashSet<Color> appendColor = new HashSet<Color>();

            if (colorCounts.Count > palColorNum - 1)
            {
                RemoveSimilarColors(colorCounts);

                Color ac = Color.Black;
                foreach (var cb in colorBase)
                {
                    int minOffset = int.MaxValue;
                    foreach (var kv in colorCounts)
                    {
                        int offset = Math.Abs(kv.Key.R - cb.R) + Math.Abs(kv.Key.G - cb.G) + Math.Abs(kv.Key.B - cb.B);
                        if (offset <= 15)
                        {
                            if (offset < minOffset)
                            {
                                minOffset = offset;
                                ac = kv.Key;
                            }
                        }
                    }

                    if (minOffset != int.MaxValue)
                    {
                        appendColor.Add(ac);
                    }
                }
            }

            var result = colorCounts.OrderByDescending(c => c.Value).Take(palColorNum - 1).Select(c => c.Key).ToList();

            if (appendColor.Count > 0)
            {
                List<Color> insertBaseColorList = new List<Color>();
                foreach (var cb in appendColor)
                {
                    bool isContains = false;
                    foreach (var cr in result)
                    {
                        if (Math.Abs(cr.R - cb.R) + Math.Abs(cr.G - cb.G) + Math.Abs(cr.B - cb.B) <= 20)
                        {
                            isContains = true;
                        }
                    }

                    if (!isContains)
                    {
                        insertBaseColorList.Add(cb);
                    }
                }

                if (insertBaseColorList.Count > 0)
                {
                    result.RemoveRange(result.Count - insertBaseColorList.Count - 1, insertBaseColorList.Count);
                    result.AddRange(insertBaseColorList);
                }
            }

            Color backgroundColor = Color.FromArgb(255, (int)Math.Floor(paletteBackground.R / 4f), (int)Math.Floor(paletteBackground.G / 4f), (int)Math.Floor(paletteBackground.B / 4f));

            result.Insert(0, backgroundColor);

            while (true)
            {
                if (result.Count >= 256)
                {
                    break;
                }

                result.Add(backgroundColor);
            }

            GC.Collect();

            return result;
        }

    }
}
