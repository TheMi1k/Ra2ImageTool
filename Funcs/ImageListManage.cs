using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Ra2ImageTool.Data;
using Ra2ImageTool.Models;

namespace Ra2ImageTool.Funcs
{
    internal class ImageListManage
    {
        internal static void LoadList(List<ImageInfoModel> list)
        {
            if (list.Count == 0)
            {
                return;
            }

            foreach (var item in GData.ListViewData)
            {
                item.DisposeImage();
                item.DisposeImageOverlay();
            }

            GData.ListViewData.Clear();

            List<string> inImgPathList = new List<string>();
            List<string> overlayImgPathList = new List<string>();

            foreach (var item in list)
            {
                inImgPathList.Add(item.InImgPath);
                overlayImgPathList.Add(item.ImgOverlayPath);
            }

            OpenPng(inImgPathList.ToArray(), 0, false);
            OpenPng(overlayImgPathList.ToArray(), 0, true);

            for (int i = 0; i < list.Count; i++)
            {
                GData.ListViewData[i].IsChanged = list[i].IsChanged;

                GData.ListViewData[i].Alpha = list[i].Alpha;
                GData.ListViewData[i].Lightness = list[i].Lightness;
                GData.ListViewData[i].TransparentDiffusion = list[i].TransparentDiffusion;
                GData.ListViewData[i].IsTransparent = list[i].IsTransparent;
                GData.ListViewData[i].OverlayMode = list[i].OverlayMode;
                GData.ListViewData[i].OverlayXOffset = list[i].OverlayXOffset;
                GData.ListViewData[i].OverlayYOffset = list[i].OverlayYOffset;
            }

            GC.Collect();
        }

        internal static void OpenPng(string[] filesPath, int startIndex, bool isOverlay)
        {
            if (GData.ListViewData.Count < startIndex + filesPath.Length)
            {
                int listCount = GData.ListViewData.Count;
                for (int i = listCount; i < startIndex + filesPath.Length; i++)
                {
                    GData.ListViewData.Add(new ImageInfoModel());
                }
            }

            for (int i = 0; i < GData.ListViewData.Count; i++)
            {
                GData.ListViewData[i].Index = i;
            }

            int insertIndex = startIndex;
            foreach (string fileName in filesPath)
            {
                bool isPng = false;
                if (!string.IsNullOrEmpty(fileName) && Path.GetExtension(fileName).Equals(".png", StringComparison.OrdinalIgnoreCase))
                {
                    isPng = true;
                }

                if (isOverlay == false)
                {
                    GData.ListViewData[insertIndex].DisposeImage();

                    if (isPng)
                    {
                        if (File.Exists(fileName))
                        {
                            using (var img = System.Drawing.Image.FromFile(fileName))
                            {
                                GData.ListViewData[insertIndex].InImg = new Bitmap(img);
                                GData.ListViewData[insertIndex].OutImg = new Bitmap(img);
                                GData.ListViewData[insertIndex].Name = Path.GetFileName(fileName);
                                GData.ListViewData[insertIndex].InImgPath = fileName;
                            }
                        }
                        else
                        {
                            GData.ListViewData[insertIndex].Name = GData.ImageFileNotExists;
                        }
                    }
                }
                else
                {
                    GData.ListViewData[insertIndex].DisposeImageOverlay();

                    if (isPng)
                    {
                        if (File.Exists(fileName))
                        {
                            using (var img = System.Drawing.Image.FromFile(fileName))
                            {
                                GData.ListViewData[insertIndex].ImgOverlay = new Bitmap(img);
                                GData.ListViewData[insertIndex].NameOverlay = Path.GetFileName(fileName);
                                GData.ListViewData[insertIndex].ImgOverlayPath = fileName;
                            }
                        }
                        else
                        {
                            GData.ListViewData[insertIndex].NameOverlay = GData.ImageFileNotExists;
                        }
                    }
                }

                insertIndex++;
            }
        }

        internal static void ClearLastEmptyItem()
        {
            if (GData.ListViewData.Count != 0)
            {
                int count = GData.ListViewData.Count - 1;
                for (int i = count; i >= 0; i--)
                {
                    if (GData.ListViewData[i].Name == "" && GData.ListViewData[i].NameOverlay == "")
                    {
                        GData.ListViewData[i].DisposeImage();
                        GData.ListViewData[i].DisposeImageOverlay();
                        GData.ListViewData.RemoveAt(i);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 删除项目，空项保留
        /// </summary>
        /// <param name="infoList"></param>
        internal static void DeleteItem(List<DataGridCellInfo> infoList)
        {
            if (infoList.Count == 0)
            {
                return;
            }

            foreach (var cellInfo in infoList)
            {
                int columnIndex = cellInfo.Column.DisplayIndex;

                if (columnIndex == 0)
                {
                    var rowData = cellInfo.Item;
                    ImageInfoModel item = rowData as ImageInfoModel;

                    item.DisposeImage();
                    item.DisposeImageOverlay();
                }
                else if (columnIndex == 1)
                {
                    var rowData = cellInfo.Item;
                    ImageInfoModel item = rowData as ImageInfoModel;

                    item.DisposeImage();
                }
                else if (columnIndex == 2)
                {
                    var rowData = cellInfo.Item;
                    ImageInfoModel item = rowData as ImageInfoModel;

                    item.DisposeImageOverlay();
                }
            }

            ClearLastEmptyItem();

            GC.Collect();
        }

        /// <summary>
        /// 删除项目，下方数据上移
        /// </summary>
        /// <param name="infoList"></param>
        internal static void DeleteItemShift(List<DataGridCellInfo> infoList)
        {
            List<ImageInfoModel> inList = new List<ImageInfoModel>();
            List<ImageInfoModel> overlayList = new List<ImageInfoModel>();

            foreach (var item in GData.ListViewData)
            {
                inList.Add(new ImageInfoModel()
                {
                    Index = item.Index,
                    Name = item.Name,
                    Alpha = item.Alpha,
                    Lightness = item.Lightness,
                    TransparentDiffusion = item.TransparentDiffusion,
                    InImg = item.InImg,
                    OutImg = item.OutImg,
                    IsChanged = item.IsChanged,
                    IsTransparent = item.IsTransparent,
                    InImgPath = item.InImgPath,
                });

                overlayList.Add(new ImageInfoModel()
                {
                    Index = item.Index,
                    NameOverlay = item.NameOverlay,
                    OverlayXOffset = item.OverlayXOffset,
                    OverlayYOffset = item.OverlayYOffset,
                    ImgOverlay = item.ImgOverlay,
                    OverlayMode = item.OverlayMode,
                    ImgOverlayPath = item.ImgOverlayPath,
                });
            }

            List<int> removeList1 = new List<int>();
            List<int> removeList2 = new List<int>();

            foreach (var cellInfo in infoList)
            {
                int columnIndex = cellInfo.Column.DisplayIndex;

                var rowData = cellInfo.Item;
                ImageInfoModel item = rowData as ImageInfoModel;

                if (columnIndex == 0)
                {
                    if (!removeList1.Contains(item.Index))
                    {
                        removeList1.Add(item.Index);
                    }
                    if (!removeList2.Contains(item.Index))
                    {
                        removeList2.Add(item.Index);
                    }
                }
                else if (columnIndex == 1)
                {
                    if (!removeList1.Contains(item.Index))
                    {
                        removeList1.Add(item.Index);
                    }
                }
                else if (columnIndex == 2)
                {
                    if (!removeList2.Contains(item.Index))
                    {
                        removeList2.Add(item.Index);
                    }
                }
            }

            removeList1.Sort((a, b) => b.CompareTo(a));
            removeList2.Sort((a, b) => b.CompareTo(a));

            foreach (var index in removeList1)
            {
                var model = inList.FirstOrDefault(x => x.Index == index);
                model.DisposeImage();
                inList.Remove(model);
            }
            foreach (var index in removeList2)
            {
                var model = overlayList.FirstOrDefault(x => x.Index == index);
                model.DisposeImageOverlay();
                overlayList.Remove(model);
            }

            for (int i = 0; i < inList.Count; i++)
            {
                GData.ListViewData[i].Name = inList[i].Name;
                GData.ListViewData[i].Alpha = inList[i].Alpha;
                GData.ListViewData[i].TransparentDiffusion = inList[i].TransparentDiffusion;
                GData.ListViewData[i].Lightness = inList[i].Lightness;
                GData.ListViewData[i].InImg = inList[i].InImg;
                GData.ListViewData[i].OutImg = inList[i].OutImg;
                GData.ListViewData[i].IsChanged = inList[i].IsChanged;
                GData.ListViewData[i].IsTransparent = inList[i].IsTransparent;
                GData.ListViewData[i].InImgPath = inList[i].InImgPath;
            }
            for (int i = inList.Count; i < GData.ListViewData.Count; i++)
            {
                GData.ListViewData[i].SetInputDefault();
            }

            for (int i = 0; i < overlayList.Count; i++)
            {
                GData.ListViewData[i].ImgOverlay = overlayList[i].ImgOverlay;
                GData.ListViewData[i].NameOverlay = overlayList[i].NameOverlay;
                GData.ListViewData[i].OverlayXOffset = overlayList[i].OverlayXOffset;
                GData.ListViewData[i].OverlayYOffset = overlayList[i].OverlayYOffset;
                GData.ListViewData[i].OverlayMode = overlayList[i].OverlayMode;
                GData.ListViewData[i].ImgOverlayPath = overlayList[i].ImgOverlayPath;
            }
            for (int i = overlayList.Count; i < GData.ListViewData.Count; i++)
            {
                GData.ListViewData[i].SetOverlayDefault();
            }

            ClearLastEmptyItem();

            // 重新设置index
            int indexNum = 0;
            foreach (var item in GData.ListViewData)
            {
                item.Index = indexNum;
                indexNum++;
            }

            GC.Collect();
        }

        internal static void InsertEmptyItem(GData.ListName cell, int startIndex, int insertCount)
        {
            if (cell == GData.ListName.空)
            {
                return;
            }

            List<ImageInfoModel> inList = new List<ImageInfoModel>();
            List<ImageInfoModel> overlayList = new List<ImageInfoModel>();

            foreach (var item in GData.ListViewData)
            {
                inList.Add(new ImageInfoModel()
                {
                    Index = item.Index,
                    Name = item.Name,
                    Alpha = item.Alpha,
                    Lightness = item.Lightness,
                    TransparentDiffusion = item.TransparentDiffusion,
                    InImg = item.InImg,
                    OutImg = item.OutImg,
                    IsChanged = item.IsChanged,
                    IsTransparent = item.IsTransparent,
                    InImgPath = item.InImgPath,
                });

                overlayList.Add(new ImageInfoModel()
                {
                    Index = item.Index,
                    NameOverlay = item.NameOverlay,
                    OverlayXOffset = item.OverlayXOffset,
                    OverlayYOffset = item.OverlayYOffset,
                    ImgOverlay = item.ImgOverlay,
                    OverlayMode = item.OverlayMode,
                    ImgOverlayPath = item.ImgOverlayPath,
                });
            }

            if (cell == GData.ListName.全部)
            {
                for (int _ = 0; _ < insertCount; _++)
                {
                    inList.Insert(startIndex, new ImageInfoModel());
                    overlayList.Insert(startIndex, new ImageInfoModel());
                }
            }
            else if (cell == GData.ListName.操作)
            {
                for (int _ = 0; _ < insertCount; _++)
                {
                    inList.Insert(startIndex, new ImageInfoModel());
                }
            }
            else
            {
                for (int _ = 0; _ < insertCount; _++)
                {
                    overlayList.Insert(startIndex, new ImageInfoModel());
                }
            }

            int maxIndex = inList.Count > overlayList.Count ? inList.Count : overlayList.Count;
            int addCount = maxIndex - GData.ListViewData.Count;
            if (maxIndex > addCount)
            {
                for (int _ = 0; _ < addCount; _++)
                {
                    GData.ListViewData.Add(new ImageInfoModel());
                }
            }

            for (int i = 0; i < inList.Count; i++)
            {
                GData.ListViewData[i].Name = inList[i].Name;
                GData.ListViewData[i].Alpha = inList[i].Alpha;
                GData.ListViewData[i].TransparentDiffusion = inList[i].TransparentDiffusion;
                GData.ListViewData[i].Lightness = inList[i].Lightness;
                GData.ListViewData[i].InImg = inList[i].InImg;
                GData.ListViewData[i].OutImg = inList[i].OutImg;
                GData.ListViewData[i].IsChanged = inList[i].IsChanged;
                GData.ListViewData[i].IsTransparent = inList[i].IsTransparent;
                GData.ListViewData[i].InImgPath = inList[i].InImgPath;
            }

            for (int i = 0; i < overlayList.Count; i++)
            {
                GData.ListViewData[i].ImgOverlay = overlayList[i].ImgOverlay;
                GData.ListViewData[i].NameOverlay = overlayList[i].NameOverlay;
                GData.ListViewData[i].OverlayXOffset = overlayList[i].OverlayXOffset;
                GData.ListViewData[i].OverlayYOffset = overlayList[i].OverlayYOffset;
                GData.ListViewData[i].OverlayMode = overlayList[i].OverlayMode;
                GData.ListViewData[i].ImgOverlayPath = overlayList[i].ImgOverlayPath;
            }

            ClearLastEmptyItem();

            // 重新设置index
            int indexNum = 0;
            foreach (var item in GData.ListViewData)
            {
                item.Index = indexNum;
                indexNum++;
            }

            GC.Collect();
        }
    }
}
