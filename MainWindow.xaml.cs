using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ra2ImageTool.Data;
using Ra2ImageTool.Funcs;
using Ra2ImageTool.Models;
using Color = System.Drawing.Color;
using static System.Net.Mime.MediaTypeNames;

namespace Ra2ImageTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string _getVersion()
            {
                string v = GData.VERSION.ToString();
                if (!v.Contains('.'))
                {
                    v += ".0";
                }

                return v;
            }

            this.Title += $" v{_getVersion()}";

            GData.UIData = new UIDataModel();
            this.DataContext = GData.UIData;

            ComboBox_OverlayMode.SelectionChanged += (sender, e)=>
            {
                if (GData.UIData.OverlayMode == GData.OverlayMode.叠加在上)
                {
                    Panel.SetZIndex(Grid_OutImg, 0);
                    Panel.SetZIndex(Grid_OutImgOverlay, 1);
                }
                else
                {
                    Panel.SetZIndex(Grid_OutImg, 1);
                    Panel.SetZIndex(Grid_OutImgOverlay, 0);
                }

                if (GData.ListViewData.Count == 0)
                {
                    return;
                }

                GData.ListViewData[GData.UIData.NowIndex].OverlayMode = GData.UIData.OverlayMode;
            };

            (StackPanel_PaletteHeaderColor.Children[0] as Button).Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 252));
            GData.PaletteConfig.PaletteHeaderColor[0] = Ra2PaletteColor.FromArgb(255, 0, 0, 252);

            ComboBox_CreatePalMode.ItemsSource = Enum.GetValues(typeof(GData.CreatePalMode));
            ComboBox_CreatePalMode.SelectedIndex = 0;

            ComboBox_OverlayMode.ItemsSource = Enum.GetValues(typeof(GData.OverlayMode));
            ComboBox_OverlayMode.SelectedIndex = 0;


#if DEBUG
            Button_Test.Visibility = Visibility.Visible;
#endif

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!ShowMessageBox("请确定当前项目是否已经保存\n是否退出？", MessageBoxButton.YesNo))
            {
                e.Cancel = true;
            }
        }

        private void SelectViewBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color selectedColor = colorDialog.Color;

                Border_InImg.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B));
                Border_OutImg.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B));
            }
        }

        private void SetBackgroundImage(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            using (var img = System.Drawing.Image.FromFile(files[0]))
            {
                // 切换为图片背景
                ImageBrush imageBrush = new ImageBrush
                {
                    ImageSource = ImageManage.ToImageSource(new Bitmap(img)),
                    Stretch = Stretch.None
                };

                Border_InImg.Background = imageBrush;
                Border_OutImg.Background = imageBrush;
            }
        }

        private void Img_Drop(object sender, DragEventArgs e)
        {
            StackPanel_Tips.Visibility = Visibility.Collapsed;

            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
            {
                return;
            }

            var point = e.GetPosition(dataGrid);
            var row = dataGrid.InputHitTest(point) as DependencyObject;

            while (row != null && !(row is DataGridRow))
            {
                row = VisualTreeHelper.GetParent(row);
            }

            int insertIndex = GData.ListViewData.Count;
            if (row is DataGridRow dataGridRow)
            {
                insertIndex = dataGrid.ItemContainerGenerator.IndexFromContainer(dataGridRow);
            }

            GData.UIData.NowIndex = 0;
            LoadImageOption(GData.UIData.NowIndex);

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            System.Windows.Point position = e.GetPosition((UIElement)sender);

            ListView_Images.ItemsSource = null;

            if (position.X <= 140)
            {
                try
                {
                    ImageListManage.OpenPng(files, insertIndex, false);

                    Image_input.Width = double.NaN;
                    Image_input.Height = double.NaN;

                    Image_input.Source = ImageManage.ToImageSource(GData.ListViewData[0].InImg);

                    Image_output.Width = double.NaN;
                    Image_output.Height = double.NaN;

                    Image_output.Source = ImageManage.ToImageSource(GData.ListViewData[0].OutImg);
                }
                catch (Exception ex)
                {
                    ListView_Images.ItemsSource = GData.ListViewData;
                    ShowMessageBox($"加载图片错误\n{ex.Message}");
                    return;
                }
            }
            else
            {
                try
                {
                    ImageListManage.OpenPng(files, insertIndex, true);

                    Image_outputOverlay.Width = double.NaN;
                    Image_outputOverlay.Height = double.NaN;

                    Image_outputOverlay.Source = ImageManage.ToImageSource(GData.ListViewData[0].ImgOverlay);
                }
                catch (Exception ex)
                {
                    ListView_Images.ItemsSource = GData.ListViewData;
                    ShowMessageBox($"加载图片错误\n{ex.Message}");
                    return;
                }
            }

            ListView_Images.ItemsSource = GData.ListViewData;
        }

        private void Img_DropToEnd(object sender, DragEventArgs e)
        {
            System.Windows.Point position = e.GetPosition((UIElement)sender);

            StackPanel_Tips.Visibility = Visibility.Collapsed;

            GData.UIData.NowIndex = 0;
            LoadImageOption(GData.UIData.NowIndex);

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            ListView_Images.ItemsSource = null;

            if (position.X <= 140)
            {
                try
                {
                    ImageListManage.OpenPng(files, GData.ListViewData.Count, false);

                    Image_input.Width = double.NaN;
                    Image_input.Height = double.NaN;

                    Image_input.Source = ImageManage.ToImageSource(GData.ListViewData[0].InImg);

                    Image_output.Width = double.NaN;
                    Image_output.Height = double.NaN;

                    Image_output.Source = ImageManage.ToImageSource(GData.ListViewData[0].OutImg);
                }
                catch (Exception ex)
                {
                    ListView_Images.ItemsSource = GData.ListViewData;
                    ShowMessageBox($"加载图片错误\n{ex.Message}");
                    return;
                }
            }
            else
            {
                try
                {
                    ImageListManage.OpenPng(files, GData.ListViewData.Count, true);

                    Image_outputOverlay.Width = double.NaN;
                    Image_outputOverlay.Height = double.NaN;

                    Image_outputOverlay.Source = ImageManage.ToImageSource(GData.ListViewData[0].ImgOverlay);
                }
                catch (Exception ex)
                {
                    ListView_Images.ItemsSource = GData.ListViewData;
                    ShowMessageBox($"加载图片错误\n{ex.Message}");
                    return;
                }
            }

            ListView_Images.ItemsSource = GData.ListViewData;
        }

        /// <summary>
        /// 处理图像并重载
        /// </summary>
        private async void ConvertImageAndReload()
        {
            if (GData.ListViewData.Count == 0)
            {
                return;
            }

            await Task.Run(() =>
            {
                GData.UIData.SetOutImageData(GData.UIData.NowIndex);
                ImageManage.ConvertImage(GData.UIData.NowIndex);
            });

            if (GData.ListViewData[GData.UIData.NowIndex].OutImg != null)
            {
                Image_output.Source = ImageManage.ToImageSource(GData.ListViewData[GData.UIData.NowIndex].OutImg);
            }
        }

        private void Button_ResetNowIndex_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ListViewData.Count == 0)
            {
                return;
            }

            try
            {
                ImageManage.DisposeBitmap(GData.ListViewData[GData.UIData.NowIndex].OutImg);

                if (GData.ListViewData[GData.UIData.NowIndex].InImg == null)
                {
                    return;
                }
                GData.ListViewData[GData.UIData.NowIndex].OutImg = new Bitmap(GData.ListViewData[GData.UIData.NowIndex].InImg);
                Image_output.Source = ImageManage.ToImageSource(GData.ListViewData[GData.UIData.NowIndex].OutImg);

                GData.ListViewData[GData.UIData.NowIndex].IsChanged = false;
            }
            catch (Exception ex)
            {
                ShowMessageBox($"重置当前图像失败\n{ex.Message}");
            }
        }

        private void Button_ResetAll_Click(object sender, RoutedEventArgs e)
        {
            GData.UIData.SetProgressUI(0, GData.ListViewData.Count);

            if (GData.ListViewData.Count == 0)
            {
                return;
            }

            try
            {
                foreach (var item in GData.ListViewData)
                {
                    ImageManage.DisposeBitmap(item.OutImg);

                    if (item.InImg != null)
                    {
                        item.OutImg = new Bitmap(item.InImg);
                        GData.ListViewData[item.Index].IsChanged = false;
                    }

                    GData.UIData.SetProgressUI(item.Index + 1, GData.ListViewData.Count);
                }

                Image_output.Source = ImageManage.ToImageSource(GData.ListViewData[GData.UIData.NowIndex].OutImg);
            }
            catch (Exception ex)
            {
                ShowMessageBox($"重置所有图像失败\n{ex.Message}");
            }
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ConvertImageAndReload();
        }

        private void LoadImageOption(int index)
        {
            try
            {
                GData.UIData.NowIndex = GData.ListViewData[index].Index;
                GData.UIData.TransparentDiffusion = GData.ListViewData[index].TransparentDiffusion;
                GData.UIData.Lightness = GData.ListViewData[index].Lightness;
                GData.UIData.Alpha = GData.ListViewData[index].Alpha;
                GData.UIData.IsTransparent = GData.ListViewData[index].IsTransparent;
                GData.UIData.ChangeOverlayMargin(GData.ListViewData[index].OverlayXOffset, GData.ListViewData[index].OverlayYOffset);
                GData.UIData.OverlayMode = GData.ListViewData[index].OverlayMode;
            }
            catch
            {

            }
        }

        private void ListView_Images_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 不是鼠标左键
            if (e.ButtonState != MouseButtonState.Pressed || e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (GData.ListViewData.Count == 0)
            {
                return;
            }

            var triggeredControl = sender as FrameworkElement;

            if (triggeredControl == null)
            {
                return;
            }

            var dataGrid = triggeredControl as System.Windows.Controls.DataGrid;
            DataGridCellInfo cellInfo = dataGrid.SelectedCells[0];
            var rowData = cellInfo.Item;

            if (rowData == null)
            {
                return;
            }

            ImageInfoModel item = rowData as ImageInfoModel;

            try
            {
                GData.UIData.NowIndex = item.Index;

                Image_input.Width = double.NaN;
                Image_input.Height = double.NaN;
                Image_input.Source = ImageManage.ToImageSource(GData.ListViewData[GData.UIData.NowIndex].InImg);

                Image_output.Width = double.NaN;
                Image_output.Height = double.NaN;
                Image_output.Source = ImageManage.ToImageSource(GData.ListViewData[GData.UIData.NowIndex].OutImg);


                Image_outputOverlay.Width = double.NaN;
                Image_outputOverlay.Height = double.NaN;
                Image_outputOverlay.Source = ImageManage.ToImageSource(GData.ListViewData[GData.UIData.NowIndex].ImgOverlay);

                LoadImageOption(item.Index);
            }
            catch
            {

            }
        }

        private async void Button_Export_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ListViewData.Count == 0)
            {
                return;
            }

            Grid_Main.IsEnabled = false;

            if (!Directory.Exists(GetPath.GetExportImagePath()))
            {
                Directory.CreateDirectory(GetPath.GetExportImagePath());
            }

            string timeStr = $"{DateTime.Now.Year}年{DateTime.Now.Month}月{DateTime.Now.Day}日{DateTime.Now.Hour}时{DateTime.Now.Minute}分{DateTime.Now.Second}秒";
            if (!Directory.Exists($@"{GetPath.GetExportImagePath()}\{timeStr}"))
            {
                Directory.CreateDirectory($@"{GetPath.GetExportImagePath()}\{timeStr}");
            }

            int sucCount = 0;
            int failCount = 0;

            GData.UIData.SetProgressUI(sucCount, GData.ListViewData.Count);

            await Task.Run(() =>
            {
                int imgOutIndex = 0;
                foreach (var item in GData.ListViewData)
                {
                    try
                    {
                        string imgName = $"{imgOutIndex.ToString().PadLeft(5, '0')}.png";

                        Bitmap result = ImageManage.MergeBitmaps(item.OutImg, item.ImgOverlay, item.OverlayXOffset, item.OverlayYOffset, item.OverlayMode);

                        if (result != null)
                        {
                            result.Save($@"{GetPath.GetExportImagePath()}\{timeStr}\{imgName}", ImageFormat.Png);
                            ImageManage.DisposeBitmap(result);
                        }

                        sucCount += 1;

                        GData.UIData.SetProgressUI(sucCount, GData.ListViewData.Count);

                        imgOutIndex += 1;
                    }
                    catch
                    {
                        failCount += 1;
                        imgOutIndex += 1;
                    }
                }
            });

            Grid_Main.IsEnabled = true;

            ShowMessageBox($"导出完毕\n成功{sucCount}, 失败{failCount}\n关闭该窗口打开输出文件夹");

            Process.Start("explorer.exe", $@"{GetPath.GetExportImagePath()}\{timeStr}");
        }

        private bool _isPlay = false;

        private async void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ListViewData.Count == 0)
            {
                _isPlay = false;
                Button_Play.Content = "播放";
                GroupBox_Change.IsEnabled = true;
                GroupBox_Export.IsEnabled = true;
                ListView_Images.IsEnabled = true;
                StackPanel_ClearList.IsEnabled = true;
                return;
            }

            if (Button_Play.Content.ToString() == "播放")
            {
                GroupBox_Change.IsEnabled = false;
                GroupBox_Export.IsEnabled = false;
                ListView_Images.IsEnabled = false;
                StackPanel_ClearList.IsEnabled = false;

                _isPlay = true;
                Button_Play.Content = "停止";
            }
            else
            {
                GroupBox_Change.IsEnabled = true;
                GroupBox_Export.IsEnabled = true;
                ListView_Images.IsEnabled = true;
                StackPanel_ClearList.IsEnabled = true;

                _isPlay = false;
                Button_Play.Content = "播放";
            }

            await Task.Run(async () =>
            {
                int gcCount = 0;
                while (true)
                {
                    try
                    {
                        if (GData.UIData.NowIndex + 1 >= GData.ListViewData.Count)
                        {
                            GData.UIData.NowIndex = 0;
                        }
                        else
                        {
                            GData.UIData.NowIndex++;
                        }

                        await Dispatcher.InvokeAsync(() =>
                        {
                            Image_input.Source = null;
                            Image_input.Width = double.NaN;
                            Image_input.Height = double.NaN;

                            Image_output.Source = null;
                            Image_output.Width = double.NaN;
                            Image_output.Height = double.NaN;

                            Image_outputOverlay.Source = null;
                            Image_outputOverlay.Width = double.NaN;
                            Image_outputOverlay.Height = double.NaN;


                            LoadImageOption(GData.UIData.NowIndex);

                            //Grid_OutImgOverlay.Margin = new Thickness(StaticData.ListViewData[UIData.NowIndex].OverlayXOffset, StaticData.ListViewData[UIData.NowIndex].OverlayYOffset, 0, 0);

                            Image_input.Source = ImageManage.ToImageSource(GData.ListViewData[GData.UIData.NowIndex].InImg);
                            Image_output.Source = ImageManage.ToImageSource(GData.ListViewData[GData.UIData.NowIndex].OutImg);
                            Image_outputOverlay.Source = ImageManage.ToImageSource(GData.ListViewData[GData.UIData.NowIndex].ImgOverlay);
                        });

                        await Task.Delay(50);

                        gcCount++;
                        if (gcCount > 5)
                        {
                            GC.Collect();
                            gcCount = 0;
                        }
                    }
                    catch
                    {

                    }

                    if (!_isPlay)
                    {
                        GC.Collect();
                        break;
                    }
                }
            });
        }

        private void Button_Test_Click(object sender, RoutedEventArgs e)
        {
            GData.UIData.NowIndex = 0;
            GData.ListViewData.Clear();
            ListView_Images.ItemsSource = null;

            StackPanel_Tips.Visibility = Visibility.Collapsed;

            string[] files = { @"C:\Users\Milk\Desktop\pi.png", @"C:\Users\Milk\Desktop\background.png", @"C:\Users\Milk\Desktop\A040001.png" };
            string[] fileso = { @"C:\Users\Milk\Desktop\00000.png", @"C:\Users\Milk\Desktop\测试图 - 副本.png" };

            try
            {
                ImageListManage.OpenPng(files, 0, false);
                ImageListManage.OpenPng(fileso, 0, true);

                Image_input.Width = double.NaN;
                Image_input.Height = double.NaN;

                Image_input.Source = ImageManage.ToImageSource(GData.ListViewData[0].InImg);

                Image_output.Width = double.NaN;
                Image_output.Height = double.NaN;

                Image_output.Source = ImageManage.ToImageSource(GData.ListViewData[0].OutImg);
            }
            catch
            {

            }

            ListView_Images.ItemsSource = GData.ListViewData;
        }

        /// <summary>
        /// 应用到所有叠加图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ApplyToAllOverlayImg_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ListViewData.Count == 0)
            {
                return;
            }

            try
            {
                for (int i = 0; i < GData.ListViewData.Count; i++)
                {
                    GData.UIData.SetOverlayData(i);
                }

                GData.UIData.ChangeOverlayMargin(GData.UIData.OverlayXOffset, GData.UIData.OverlayYOffset);

                ShowMessageBox("更改完成");
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }
        }

        /// <summary>
        /// 应用到所有操作图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_ApplyToAllImg_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ListViewData.Count == 0)
            {
                return;
            }

            Grid_Main.IsEnabled = false;

            int maxCount = GData.ListViewData.Count;
            int sucCount = 0;

            GData.UIData.SetProgressUI(0, maxCount);

            //Stopwatch sw = Stopwatch.StartNew();
            //sw.Start();

            await Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();

                for (int i = 0; i < GData.ListViewData.Count; i++)
                {
                    int j = i;
                    tasks.Add(Task.Run(() =>
                    {
                        GData.UIData.SetOutImageData(j);
                        ImageManage.ConvertImage(j);

                        lock (GData.Locker_changeAllImg)
                        {
                            sucCount++;
                            GData.UIData.SetProgressUI(sucCount, maxCount);
                        }
                    }));
                }

                Task.WaitAll(tasks.ToArray());
            });

            //sw.Stop();

            //Console.WriteLine($"{sw.ElapsedTicks} | {sw.ElapsedMilliseconds}");

            GC.Collect();

            if (GData.ListViewData[GData.UIData.NowIndex].OutImg != null)
            {
                Image_output.Source = ImageManage.ToImageSource(GData.ListViewData[GData.UIData.NowIndex].OutImg);
            }

            Grid_Main.IsEnabled = true;

            ShowMessageBox("应用到所有图像完成");
        }

        private async void Button_CreatePal_Click(object sender, RoutedEventArgs e)
        {
            int palColorNum;
            try
            {
                palColorNum = int.Parse(TextBox_PalColorNum.Text);
                if (palColorNum < 2 || palColorNum > 256)
                {
                    throw new Exception("色盘颜色数量只能为2-256");
                }

                if (GData.ListViewData.Count == 0)
                {
                    return;
                }

                Grid_Main.IsEnabled = false;

                bool isPlayerColor = (bool)CheckBox_PlayerColor.IsChecked;

                List<Ra2PaletteColor> palette = await ImageManage.CreatePalette(palColorNum, GData.PaletteConfig.PaletteHeaderColor, isPlayerColor ? GData.PaletteConfig.PalettePlayerColor : null, ComboBox_CreatePalMode.SelectedItem.ToString());

                CreatePaletteViewWindow window = new CreatePaletteViewWindow(palette);
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Owner = this;
                window.ShowDialog();

                if (!window.Result)
                {
                    Grid_Main.IsEnabled = true;
                    return;
                }

                if (!Directory.Exists($@"{GetPath.GetExportPalPath()}"))
                {
                    Directory.CreateDirectory($@"{GetPath.GetExportPalPath()}");
                }

                string timeStr = $"{DateTime.Now.Year}年{DateTime.Now.Month}月{DateTime.Now.Day}日{DateTime.Now.Hour}时{DateTime.Now.Minute}分{DateTime.Now.Second}秒";
                if (!Directory.Exists($@"{GetPath.GetExportPalPath()}\{timeStr}"))
                {
                    Directory.CreateDirectory($@"{GetPath.GetExportPalPath()}\{timeStr}");
                }
                using (BinaryWriter writer = new BinaryWriter(File.Open($@"{GetPath.GetExportPalPath()}\{timeStr}\色盘.pal", FileMode.Create)))
                {
                    foreach (var color in palette)
                    {
                        writer.Write((byte)color.R);
                        writer.Write((byte)color.G);
                        writer.Write((byte)color.B);
                    }
                }

                ShowMessageBox("完成\n关闭该窗口打开输出文件夹");

                Process.Start("explorer.exe", $@"{GetPath.GetExportPalPath()}\{timeStr}");
            }
            catch (Exception ex)
            {
                ShowMessageBox($"生成色盘失败\n{ex.Message}");
            }

            Grid_Main.IsEnabled = true;
        }

        private bool ShowMessageBox(string text, MessageBoxButton type = MessageBoxButton.OK)
        {
            try
            {
                var window = new MyMessageBox(text, type);
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Owner = this;
                window.ShowDialog();

                return window.Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("载入提示框失败\n" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 预览窗口透明度更改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_ArtTranslucencyChanged(object sender, RoutedEventArgs e)
        {
            if (RadioButton_ArtTranslucency_none.IsChecked == true)
            {
                Image_output.Opacity = 1.0;
            }
            else if (RadioButton_ArtTranslucency_25.IsChecked == true)
            {
                Image_output.Opacity = 0.75;
            }
            else if (RadioButton_ArtTranslucency_50.IsChecked == true)
            {
                Image_output.Opacity = 0.5;
            }
            else if (RadioButton_ArtTranslucency_75.IsChecked == true)
            {
                Image_output.Opacity = 0.25;
            }
        }

        private void Button_ClearList1_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ListViewData.Count != 0)
            {
                try
                {
                    foreach (var item in GData.ListViewData)
                    {
                        item.DisposeImage();
                    }

                    ImageListManage.ClearLastEmptyItem();

                    ListView_Images.ItemsSource = null;
                    ListView_Images.ItemsSource = GData.ListViewData;

                    GData.UIData.NowIndex = 0;
                    Image_input.Source = null;
                    Image_output.Source = null;

                    GC.Collect();
                }
                catch (Exception ex)
                {
                    ShowMessageBox(ex.Message);
                    return;
                }
            }
        }

        private void Button_ClearList2_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ListViewData.Count != 0)
            {
                try
                {
                    foreach (var item in GData.ListViewData)
                    {
                        item.DisposeImageOverlay();
                    }

                    ImageListManage.ClearLastEmptyItem();

                    ListView_Images.ItemsSource = null;
                    ListView_Images.ItemsSource = GData.ListViewData;

                    GData.UIData.NowIndex = 0;
                    Image_outputOverlay.Source = null;

                    GC.Collect();
                }
                catch (Exception ex)
                {
                    ShowMessageBox(ex.Message);
                    return;
                }
            }
        }

        private void ListView_Images_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ImageListManage.DeleteItemShift(ListView_Images.SelectedCells.ToList());

                ListView_Images.ItemsSource = null;
                ListView_Images.ItemsSource = GData.ListViewData;

                GData.UIData.NowIndex = 0;
                Image_input.Source = null;
                Image_output.Source = null;
                Image_outputOverlay.Source = null;

                GData.UIData.SetAllDefault();

                return;
            }

            if (e.Key == Key.Delete)
            {
                ImageListManage.DeleteItem(ListView_Images.SelectedCells.ToList());

                ListView_Images.ItemsSource = null;
                ListView_Images.ItemsSource = GData.ListViewData;

                GData.UIData.NowIndex = 0;
                Image_input.Source = null;
                Image_output.Source = null;
                Image_outputOverlay.Source = null;

                GData.UIData.SetAllDefault();

                return;
            }
        }

        private void CheckBox_OutlineTransparent_Changed(object sender, RoutedEventArgs e)
        {
            if (GData.ListViewData.Count == 0)
            {
                return;
            }

            bool now = GData.ListViewData[GData.UIData.NowIndex].IsTransparent;

            if (CheckBox_OutlineTransparent.IsChecked == true)
            {
                //Slider_OutlineTransparentOffset.IsEnabled = true;
                GData.ListViewData[GData.UIData.NowIndex].IsTransparent = true;
                GData.UIData.IsTransparent = true;
            }
            else
            {
                //Slider_OutlineTransparentOffset.IsEnabled = false;
                GData.ListViewData[GData.UIData.NowIndex].IsTransparent = false;
                GData.UIData.IsTransparent = false;
            }

            if (now != GData.ListViewData[GData.UIData.NowIndex].IsTransparent)
            {
                ConvertImageAndReload();
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            e.Handled = !Regex.IsMatch(newText, @"^-?\d*$");
        }

        private int _insertIndex = -1;
        private GData.ListName _insertListName = GData.ListName.空;
        private void DataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _insertIndex = -1;
            _insertListName = GData.ListName.空;

            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            // 获取鼠标点击的位置
            var point = e.GetPosition(dataGrid);
            var hit = dataGrid.InputHitTest(point) as DependencyObject;

            while (hit != null && !(hit is DataGridRow))
            {
                hit = VisualTreeHelper.GetParent(hit);
            }

            if (hit is DataGridRow dataGridRow)
            {
                _insertIndex = dataGrid.ItemContainerGenerator.IndexFromContainer(dataGridRow);
                if (point.X < 41)
                {
                    MenuItem1.Header = $"往两个列表 {_insertIndex.ToString().PadLeft(5, '0')} 位置插入行";
                    _insertListName = GData.ListName.全部;
                }
                else if (point.X < 141)
                {
                    MenuItem1.Header = $"往操作列表 {_insertIndex.ToString().PadLeft(5, '0')} 位置插入行";
                    _insertListName = GData.ListName.操作;
                }
                else
                {
                    MenuItem1.Header = $"往叠加列表 {_insertIndex.ToString().PadLeft(5, '0')} 位置插入行";
                    _insertListName = GData.ListName.叠加;
                }
            }
            else
            {
                dataGridContextMenu.IsOpen = false;
                return;
            }

            if (hit is DataGridRow || hit is System.Windows.Controls.DataGridCell)
            {
                // 在鼠标右键点击位置显示上下文菜单
                dataGridContextMenu.PlacementTarget = dataGrid;
                dataGridContextMenu.IsOpen = true;
            }
            else
            {
                dataGridContextMenu.IsOpen = false;
            }
        }

        private void InsertMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_insertIndex == -1 || _insertListName == GData.ListName.空)
            {
                return;
            }

            var window = new ListInsertWindow(_insertIndex, _insertListName.ToString());
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;
            window.ShowDialog();

            if (window.InsertLineCount > 0)
            {
                ImageListManage.InsertEmptyItem(_insertListName, _insertIndex, window.InsertLineCount);

                ListView_Images.ItemsSource = null;
                ListView_Images.ItemsSource = GData.ListViewData;
            }

            _insertIndex = -1;
            _insertListName = GData.ListName.空;
        }

        private void DataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        private void MenuItem_SaveProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GData.ListViewData.Count == 0)
                {
                    throw new Exception("当前项目为空");
                }

                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "选择保存路径",
                    Filter = "ImageToolJson文件(*.itjson)|*.itjson",
                    FileName = string.Empty,
                    RestoreDirectory = true,
                    DefaultExt = "itjson"
                };

                if (saveFileDialog.ShowDialog() == false)
                {
                    return;
                }

                string fileName = saveFileDialog.FileName;

                var jsonStr = JsonConvert.SerializeObject(GData.ListViewData, Formatting.Indented);

                using(StreamWriter sw = new StreamWriter(fileName))
                {
                    sw.Write(jsonStr);
                }

                ShowMessageBox("保存成功");
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }
        }

        private async void MenuItem_OpenProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GData.ListViewData.Count > 0)
                {
                    if (!ShowMessageBox("打开项目会覆盖当前项目\n是否继续？", MessageBoxButton.YesNo))
                    {
                        return;
                    }
                }

                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "选择打开文件",
                    Filter = "ImageToolJson文件(*.itjson)|*.itjson",
                    FileName = string.Empty,
                    RestoreDirectory = true,
                    DefaultExt = "itjson"
                };

                if (openFileDialog.ShowDialog() == false)
                {
                    return;
                }

                string fileName = openFileDialog.FileName;

                var jsonStr = string.Empty;

                using (StreamReader sr = new StreamReader(fileName))
                {
                    jsonStr = sr.ReadToEnd();
                }

                if (string.IsNullOrEmpty(jsonStr))
                {
                    throw new Exception("打开的文件内容不正确");
                }

                GData.UIData.NowIndex = 0;
                Image_input.Source = null;
                Image_output.Source = null;
                Image_outputOverlay.Source = null;

                GC.Collect();

                Grid_Main.IsEnabled = false;

                ImageListManage.LoadList(JsonConvert.DeserializeObject<List<ImageInfoModel>>(jsonStr));

                ListView_Images.ItemsSource = null;
                ListView_Images.ItemsSource = GData.ListViewData;

                StackPanel_Tips.Visibility = Visibility.Collapsed;

                if (GData.ListViewData.Count == 0)
                {
                    throw new Exception("打开的项目里没有任何图片");
                }

                int maxCount = GData.ListViewData.Count;
                int sucCount = 0;

                GData.UIData.SetProgressUI(0, maxCount);

                await Task.Run(() =>
                {
                    List<Task> tasks = new List<Task>();

                    for (int i = 0; i < GData.ListViewData.Count; i++)
                    {
                        int j = i;
                        tasks.Add(Task.Run(() =>
                        {
                            if (GData.ListViewData[j].IsChanged)
                            {
                                ImageManage.ConvertImage(j);
                            }

                            lock (GData.Locker_changeAllImg)
                            {
                                sucCount++;
                                GData.UIData.SetProgressUI(sucCount, maxCount);
                            }
                        }));
                    }

                    Task.WaitAll(tasks.ToArray());
                });

                GC.Collect();

                GData.UIData.SetAllDefault();

                ShowMessageBox("打开完毕");
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }

            Grid_Main.IsEnabled = true;
        }

        private void MenuItem_NewProject_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ListViewData.Count > 0)
            {
                if (!ShowMessageBox("新建项目会覆盖当前项目\n是否继续？", MessageBoxButton.YesNo))
                {
                    return;
                }

                Image_input.Source = null;
                Image_input.Width = double.NaN;
                Image_input.Height = double.NaN;

                Image_output.Source = null;
                Image_output.Width = double.NaN;
                Image_output.Height = double.NaN;

                Image_outputOverlay.Source = null;
                Image_outputOverlay.Width = double.NaN;
                Image_outputOverlay.Height = double.NaN;

                foreach (var item in GData.ListViewData)
                {
                    item.DisposeImage();
                    item.DisposeImageOverlay();
                }

                GData.ListViewData.Clear();
                GData.UIData.SetAllDefault();

                GC.Collect();
            }
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            var window = new AboutWindow();
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;
            window.ShowDialog();
        }

        private void Drop_GifToPng(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (string.IsNullOrEmpty(files[0]) || !Path.GetExtension(files[0]).Equals(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("指定图片不是Gif");
                }

                if (!Directory.Exists($@"{GetPath.GetGifToPngPath()}"))
                {
                    Directory.CreateDirectory($@"{GetPath.GetGifToPngPath()}");
                }

                string timeStr = $"{DateTime.Now.Year}年{DateTime.Now.Month}月{DateTime.Now.Day}日{DateTime.Now.Hour}时{DateTime.Now.Minute}分{DateTime.Now.Second}秒";
                if (!Directory.Exists($@"{GetPath.GetGifToPngPath()}\{timeStr}"))
                {
                    Directory.CreateDirectory($@"{GetPath.GetGifToPngPath()}\{timeStr}");
                }

                using (System.Drawing.Image image = System.Drawing.Image.FromFile(files[0]))
                {
                    FrameDimension fd = new FrameDimension(image.FrameDimensionsList[0]);
                    int frameCount = image.GetFrameCount(fd);

                    GData.UIData.SetProgressUI(0, frameCount);
                    for (int i = 0; i < frameCount; i++)
                    {
                        image.SelectActiveFrame(fd, i);
                        using (Bitmap bitmap = new Bitmap(image))
                        {
                            bitmap.Save($@"{GetPath.GetGifToPngPath()}\{timeStr}\{i.ToString().PadLeft(5, '0')}.png", ImageFormat.Png);
                            GData.UIData.SetProgressUI(i + 1, frameCount);
                        }
                    }
                }

                ShowMessageBox($"转换成功\n关闭该窗口打开输出文件夹");

                Process.Start("explorer.exe", $@"{GetPath.GetGifToPngPath()}\{timeStr}");
            }
            catch (Exception ex)
            {
                ShowMessageBox(ex.Message);
            }
        }

        private void Button_EditPlayerColor_Click(object sender, RoutedEventArgs e)
        {
            if (GData.ListViewData.Count == 0)
            {
                return;
            }

            if (GData.ListViewData[GData.UIData.NowIndex].OutImg == null && GData.ListViewData[GData.UIData.NowIndex].ImgOverlay == null)
            {
                return;
            }

            var window = new PlayerColorEditWindow(GData.UIData.NowIndex);
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;
            window.ShowDialog();
        }

        private void StackPanel_PaletteColorNum_MouseEnter(object sender, MouseEventArgs e)
        {
            Border_PaletteTip.Visibility = Visibility.Visible;
        }

        private void StackPanel_PaletteColorNum_MouseLeave(object sender, MouseEventArgs e)
        {
            Border_PaletteTip.Visibility = Visibility.Collapsed;
        }

        private void ArtTranslucency_MouseEnter(object sender, MouseEventArgs e)
        {
            Border_ArtTranslucencyTip.Visibility = Visibility.Visible;
        }

        private void ArtTranslucency_MouseLeave(object sender, MouseEventArgs e)
        {
            Border_ArtTranslucencyTip.Visibility = Visibility.Collapsed;
        }

        private void ComboBox_CreatePalMode_MouseEnter(object sender, MouseEventArgs e)
        {
            Border_CreatePaletteModeTip.Visibility = Visibility.Visible;
        }

        private void ComboBox_CreatePalMode_MouseLeave(object sender, MouseEventArgs e)
        {
            Border_CreatePaletteModeTip.Visibility= Visibility.Collapsed;
        }

        private void CheckBox_PlayerColor_Changed(object sender, RoutedEventArgs e)
        {
            if (CheckBox_PlayerColor.IsChecked == true)
            {
                Button_EditPlayerColor.IsEnabled = true;
            }
            else
            {
                Button_EditPlayerColor.IsEnabled = false;
            }

            if (GData.PaletteConfig.PalettePlayerColor.Count < 16)
            {
                GData.PaletteConfig.PalettePlayerColor.Count();

                for (int _ = 0; _ < 16; _++)
                {
                    GData.PaletteConfig.PalettePlayerColor.Add(Ra2PaletteColor.FromArgb(0, 0, 0, 0));
                }
            }
        }

        private void Button_SetPalHeaderColor_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index = StackPanel_PaletteHeaderColor.Children.IndexOf(button);

            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Ra2PaletteColor selectedColor = Ra2PaletteColor.FromColor(colorDialog.Color);

                GData.PaletteConfig.PaletteHeaderColor[index] = selectedColor;
                (StackPanel_PaletteHeaderColor.Children[index] as Button).Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)(selectedColor.R * 4), (byte)(selectedColor.G * 4), (byte)(selectedColor.B * 4)));
            }
        }

        private void ClearPalHeaderColor(object sender, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            int index = StackPanel_PaletteHeaderColor.Children.IndexOf(button);
            GData.PaletteConfig.PaletteHeaderColor[index] = Ra2PaletteColor.FromArgb(0, 0, 0, 0);
            (StackPanel_PaletteHeaderColor.Children[index] as Button).Background = System.Windows.Media.Brushes.Transparent;
        }
    }
}
