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
                    frames.Add(ImageTypeConvert.BitmapSourceToBitmap(frame));
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

        private static double[,] GetDistanceMatrix(Bitmap bitmap)
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
                byte alpha = rgbValues[i + 3];

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
                outImgBitmapSource = ImageTypeConvert.BitmapToBitmapSource(outImg);
            }

            BitmapSource overlayImgBitmapSource = ImageTypeConvert.BitmapToBitmapSource(overlayImg);

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

            return ImageTypeConvert.BitmapSourceToBitmap(mergedBitmap);
        }

        /// <summary>
        /// 移除相似颜色
        /// </summary>
        /// <param name="colorDic"></param>
        private static void RemoveSimilarColors(Dictionary<Ra2PaletteColor, int> colorDic)
        {
            List<Ra2PaletteColor> allColorList = new List<Ra2PaletteColor>();
            HashSet<Ra2PaletteColor> removeList = new HashSet<Ra2PaletteColor>();

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

        private static void RemovePlayerColors(List<Ra2PaletteColor> colors, List<Ra2PaletteColor> playerColors)
        {
            if (colors.Count == 0)
            {
                return;
            }

            List<Ra2PaletteColor> removeList = new List<Ra2PaletteColor>();

            foreach (var c in colors)
            {
                foreach (var pc in playerColors)
                {
                    if (pc.A == 0)
                    {
                        continue;
                    }

                    if (Math.Abs(c.R - pc.R) + Math.Abs(c.G - pc.G) + Math.Abs(c.B - pc.B) <= 15)
                    {
                        removeList.Add(c);
                        break;
                    }
                }
            }

            foreach (var item in removeList)
            {
                colors.Remove(item);
            }
        }

        private static void RemovePlayerColors(HashSet<Color> colors, List<Ra2PaletteColor> playerColors)
        {
            if (colors.Count == 0)
            {
                return;
            }

            List<Color> removeList = new List<Color>();

            foreach (var c in colors)
            {
                foreach (var pc in playerColors)
                {
                    if (pc.A == 0)
                    {
                        continue;
                    }

                    if (Math.Abs((c.R / 4) - pc.R) + Math.Abs((c.G / 4) - pc.G) + Math.Abs((c.B / 4) - pc.B) <= 15)
                    {
                        removeList.Add(c);
                        break;
                    }
                }
            }

            foreach (var item in removeList)
            {
                colors.Remove(item);
            }
        }

        internal static async Task<List<Ra2PaletteColor>> CreatePalette(int palColorNum, Ra2PaletteColor[] paletteHeaderColor, List<Ra2PaletteColor> playerColorList, string mode)
        {
            List<Ra2PaletteColor> playerColorSetBackgroud = null;
            if (playerColorList != null)
            {
                playerColorSetBackgroud = new List<Ra2PaletteColor>();
                foreach (var color in playerColorList)
                {
                    if (color.A == 0)
                    {
                        playerColorSetBackgroud.Add(paletteHeaderColor[0]);
                    }
                    else
                    {
                        playerColorSetBackgroud.Add(color);
                    }
                }
            }

            List<Ra2PaletteColor> headerColors = new List<Ra2PaletteColor>();
            foreach (var phc in paletteHeaderColor)
            {
                if (phc.A != 0)
                {
                    headerColors.Add(phc);
                }
            }

            if (mode == GData.CreatePalMode.主要颜色补小像素.ToString())
            {
                if (palColorNum < 150)
                {
                    throw new Exception("该色盘生成方法颜色数量不能小于150");
                }
                return await Create256ColorPalette_Mode3(palColorNum, headerColors.ToArray(), playerColorSetBackgroud);
            }
            else if (mode == GData.CreatePalMode.主要颜色.ToString())
            {
                return await Create256ColorPalette_Mode1(palColorNum, headerColors.ToArray(), playerColorSetBackgroud);
            }
            else
            {
                return await Create256ColorPalette_Mode2(palColorNum, headerColors.ToArray(), playerColorSetBackgroud);
            }
        }

        /// <summary>
        /// 生成色盘(主要颜色优先)
        /// </summary>
        /// <param name="palColorNum"></param>
        /// <returns></returns>
        private static async Task<List<Ra2PaletteColor>> Create256ColorPalette_Mode1(int palColorNum, Ra2PaletteColor[] paletteHeaderColor, List<Ra2PaletteColor> playerColorList)
        {
            Dictionary<Ra2PaletteColor, int> colorCounts = new Dictionary<Ra2PaletteColor, int>();

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

                            Ra2PaletteColor c = Ra2PaletteColor.FromArgb(255, red, green, blue);

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

            RemoveSimilarColors(colorCounts);

            var result = colorCounts.OrderByDescending(c => c.Value)
                              .Select(c => c.Key)
                              .ToList();

            if (playerColorList != null)
            {
                RemovePlayerColors(result, playerColorList);
            }

            // 插入头部颜色
            for (int i = paletteHeaderColor.Length - 1; i >= 0; i--)
            {
                result.Insert(0, paletteHeaderColor[i]);
            }

            // 插入玩家所属色
            if (playerColorList != null)
            {
                result.InsertRange(16, playerColorList);
            }

            result = result.Take(palColorNum).ToList();

            while (true)
            {
                if (result.Count >= 256)
                {
                    break;
                }

                result.Add(paletteHeaderColor[0]);
            }

            GC.Collect();

            return result;
        }

        /// <summary>
        /// 生成色盘(OctreeQuantizer)
        /// </summary>
        /// <param name="palColorNum"></param>
        /// <returns></returns>
        private static async Task<List<Ra2PaletteColor>> Create256ColorPalette_Mode2(int palColorNum, Ra2PaletteColor[] paletteHeaderColor, List<Ra2PaletteColor> playerColorList)
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

            int numSub = 0;
            if (playerColorList != null)
            {
                RemovePlayerColors(hs, playerColorList);
                numSub = 16;
            }

            Bitmap allColorBitmap = new Bitmap(hs.Count, 1);
            int count = 0;
            foreach (var c in hs)
            {
                allColorBitmap.SetPixel(count, 0, c);
                count++;
            }

            ImageFactory factory = new ImageFactory();
            factory.Load(allColorBitmap);
            ISupportedImageFormat format = new PngFormat { Quality = 100, IsIndexed = true, Quantizer = new OctreeQuantizer(palColorNum - paletteHeaderColor.Length - numSub, 8) };
            factory.Format(format);
            MemoryStream stream = new MemoryStream();
            factory.Save(stream);
            Bitmap src = new Bitmap(stream);
            stream.Dispose();
            DisposeBitmap(allColorBitmap);

            HashSet<Ra2PaletteColor> set = new HashSet<Ra2PaletteColor>();

            for (int x = 0; x < src.Width; x++)
            {
                set.Add(Ra2PaletteColor.FromColor(src.GetPixel(x, 0)));
            }

            DisposeBitmap(src);

            var result = set.ToList();

            // 插入头部颜色
            for (int i = paletteHeaderColor.Length - 1; i >= 0; i--)
            {
                result.Insert(0, paletteHeaderColor[i]);
            }

            // 插入玩家所属色
            if (playerColorList != null && playerColorList.Count == 16)
            {
                result.InsertRange(16, playerColorList);
            }

            result = result.Take(palColorNum).ToList();

            while (true)
            {
                if (result.Count >= 256)
                {
                    break;
                }

                result.Add(paletteHeaderColor[0]);
            }

            GC.Collect();

            return result;
        }

        /// <summary>
        /// 生成色盘(补全少部分像素细节)
        /// </summary>
        /// <param name="palColorNum"></param>
        /// <returns></returns>
        private static async Task<List<Ra2PaletteColor>> Create256ColorPalette_Mode3(int palColorNum, Ra2PaletteColor[] paletteHeaderColor, List<Ra2PaletteColor> playerColorList)
        {
            Dictionary<Ra2PaletteColor, int> colorCounts = new Dictionary<Ra2PaletteColor, int>();

            int sucCount = 0;
            int maxCount = GData.ListViewData.Count;

            GData.UIData.SetProgressUI(sucCount, maxCount);

            List<Ra2PaletteColor> colorBase = new List<Ra2PaletteColor>();
            for (byte r = 4; r <= 244; r += 60)
            {
                for (byte g = 4; g <= 244; g += 60)
                {
                    for (byte b = 4; b <= 244; b += 60)
                    {
                        colorBase.Add(Ra2PaletteColor.FromArgb(255, r, g, b));
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

                            Ra2PaletteColor c = Ra2PaletteColor.FromArgb(255, red, green, blue);

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

            HashSet<Ra2PaletteColor> appendColor = new HashSet<Ra2PaletteColor>();

            if (colorCounts.Count > palColorNum - 1)
            {
                RemoveSimilarColors(colorCounts);

                // 获取原图片中对于base颜色中最相近的颜色
                Ra2PaletteColor ac = Ra2PaletteColor.FromArgb(0, 0, 0, 0);
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

            var result = colorCounts.OrderByDescending(c => c.Value).Select(c => c.Key).ToList();

            if (playerColorList != null)
            {
                RemovePlayerColors(result, playerColorList);
            }

            // 插入头部颜色
            for (int i = paletteHeaderColor.Length - 1; i >= 0; i--)
            {
                result.Insert(0, paletteHeaderColor[i]);
            }

            // 插入玩家所属色
            if (playerColorList != null)
            {
                result.InsertRange(16, playerColorList);
            }

            result = result.Take(palColorNum).ToList();

            // 插入细节颜色
            if (appendColor.Count > 0)
            {
                // 判断当前色盘中有没有这个色
                List<Ra2PaletteColor> insertBaseColorList = new List<Ra2PaletteColor>();
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

                if (playerColorList != null)
                {
                    RemovePlayerColors(insertBaseColorList, playerColorList);
                }

                if (insertBaseColorList.Count > 0)
                {
                    result.RemoveRange(result.Count - insertBaseColorList.Count - 1, insertBaseColorList.Count);
                    result.AddRange(insertBaseColorList);
                }
            }

            while (true)
            {
                if (result.Count >= 256)
                {
                    break;
                }

                result.Add(paletteHeaderColor[0]);
            }

            GC.Collect();

            return result;
        }
    }
}
