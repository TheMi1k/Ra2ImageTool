using ImageProcessor.Imaging.Colors;
using Ra2ImageTool.Data;
using Ra2ImageTool.Funcs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ra2ImageTool
{
    /// <summary>
    /// CreatePaletteSetWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PlayerColorEditWindow : Window
    {
        private Bitmap _bitmap;
        private int _selectIndex = -1;
        private Ra2PaletteColor[] _playerColors = new Ra2PaletteColor[16];

        public PlayerColorEditWindow(int imgIndex)
        {
            InitializeComponent();

            Border_Magnify.Visibility = Visibility.Collapsed;
            StackPanel_NowSelectIndexManage.Visibility = Visibility.Hidden;

            _bitmap = ImageManage.MergeBitmaps(GData.ListViewData[imgIndex].OutImg, GData.ListViewData[imgIndex].ImgOverlay, GData.ListViewData[imgIndex].OverlayXOffset, GData.ListViewData[imgIndex].OverlayYOffset, GData.ListViewData[imgIndex].OverlayMode);

            Image_SetPlayerColor.Width = double.NaN;
            Image_SetPlayerColor.Height = double.NaN;

            Image_SetPlayerColor.Source = ImageManage.ToImageSource(_bitmap);

            Image_ImgMagnify.Width = _bitmap.Width * 17;
            Image_ImgMagnify.Height = _bitmap.Height * 17;

            Image_ImgMagnify.Source = ImageManage.ToImageSource(_bitmap);

            LoadColor(GData.PaletteConfig.PalettePlayerColor);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _bitmap.Dispose();

            GC.Collect();

            base.OnClosing(e);
        }

        private void LoadColor(List<Ra2PaletteColor> colors)
        {
            if (colors.Count < 16)
            {
                return;
            }

            for (int i = 0; i < 16; i++)
            {
                _playerColors[i] = colors[i];

                Button btn = StackPanel_Colors.Children[i] as Button;
                btn.Background = Ra2PaletteColorToBrush(colors[i]);
            }
        }

        private SolidColorBrush Ra2PaletteColorToBrush(Ra2PaletteColor color)
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, (byte)(color.R * 4), (byte)(color.G * 4), (byte)(color.B * 4)));
        }

        private void Button_SelectColor_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int index = StackPanel_Colors.Children.IndexOf(button);

            if (_selectIndex != -1 && _selectIndex == index)
            {
                Border_Magnify.Visibility = Visibility.Collapsed;
                StackPanel_NowSelectIndexManage.Visibility = Visibility.Hidden;

                _selectIndex = -1;
                return;
            }

            _selectIndex = index;

            StackPanel_NowSelectIndexManage.Margin = new Thickness(5, index * 20, 0, 0);
            StackPanel_NowSelectIndexManage.Visibility = Visibility.Visible;
        }

        private void Image_SetPlayerColor_MouseLeave(object sender, MouseEventArgs e)
        {
            Border_Magnify.Visibility = Visibility.Collapsed;
        }

        private void Image_SetPlayerColor_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_selectIndex < 0)
            {
                return;
            }

            Border_Magnify.Visibility = Visibility.Visible;

            var p_border = e.GetPosition(Border_Image);
            var p_img = e.GetPosition(Image_SetPlayerColor);

            SetMagnifyView(p_border, p_img);
        }

        private void Image_SetPlayerColor_MouseMove(object sender, MouseEventArgs e)
        {
            if (_selectIndex < 0)
            {
                return;
            }

            var p_border = e.GetPosition(Border_Image);
            var p_img = e.GetPosition(Image_SetPlayerColor);

            SetMagnifyView(p_border, p_img);
        }

        private void SetMagnifyView(System.Windows.Point point_border, System.Windows.Point point_img)
        {
            double MagnifyTop;
            double MagnifyLeft;

            int offset = 10;

            if (point_border.X + Border_Magnify.Width + offset > Border_Image.Width)
            {
                MagnifyLeft = point_border.X - Border_Magnify.Width - offset;
            }
            else
            {
                MagnifyLeft = point_border.X + offset;
            }

            if (point_border.Y + Border_Magnify.Height + offset > Border_Image.Height)
            {
                MagnifyTop = point_border.Y - Border_Magnify.Height - offset;
            }
            else
            {
                MagnifyTop = point_border.Y + offset;
            }

            Border_Magnify.Margin = new Thickness(MagnifyLeft, MagnifyTop, 0, 0);
            Image_ImgMagnify.Margin = new Thickness(((int)point_img.X - 4) * -17, ((int)point_img.Y - 4) * -17, 0, 0);
        }

        /// <summary>
        /// 选定颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_SetPlayerColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectIndex < 0)
            {
                return;
            }

            var p_img = e.GetPosition(Image_SetPlayerColor);
            var p = _bitmap.GetPixel((int)p_img.X, (int)p_img.Y);

            if (p.A == 0)
            {
                return;
            }

            Ra2PaletteColor color = Ra2PaletteColor.FromColor(p);

            Button btn = StackPanel_Colors.Children[_selectIndex] as Button;
            btn.Background = Ra2PaletteColorToBrush(color);

            _playerColors[_selectIndex] = color;

            _selectIndex = -1;

            Border_Magnify.Visibility = Visibility.Collapsed;
            StackPanel_NowSelectIndexManage.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 取消选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_SetPlayerColor_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_selectIndex < 0)
            {
                return;
            }

            Border_Magnify.Visibility = Visibility.Collapsed;
            StackPanel_NowSelectIndexManage.Visibility = Visibility.Hidden;

            _selectIndex = -1;
        }

        private void Button_CreateColors_Click(object sender, RoutedEventArgs e)
        {
            if (_playerColors[0].A == 0 || _playerColors[_playerColors.Length - 1].A == 0)
            {
                return;
            }

            Ra2PaletteColor firstColor = _playerColors[0];
            Ra2PaletteColor lastColor = _playerColors[_playerColors.Length - 1];

            double stepR = ((firstColor.R * 4) - (lastColor.R * 4)) / 15;
            double stepG = ((firstColor.G * 4) - (lastColor.G * 4)) / 15;
            double stepB = ((firstColor.B * 4) - (lastColor.B * 4)) / 15;

            for (int i = 1; i <= 14; i++)
            {
                byte r = (byte)Clamp((firstColor.R * 4) - (int)Math.Round(stepR * i));
                byte g = (byte)Clamp((firstColor.G * 4) - (int)Math.Round(stepG * i));
                byte b = (byte)Clamp((firstColor.B * 4) - (int)Math.Round(stepB * i));

                _playerColors[i] = Ra2PaletteColor.FromArgb(255, r, g, b);
                Button btn = StackPanel_Colors.Children[i] as Button;
                btn.Background = Ra2PaletteColorToBrush(_playerColors[i]);
            }
        }

        private static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        private void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            _playerColors[_selectIndex] = Ra2PaletteColor.FromArgb(0, 0, 0, 0);
            Button btn = StackPanel_Colors.Children[_selectIndex] as Button;
            btn.Background = Ra2PaletteColorToBrush(_playerColors[_selectIndex]);
        }

        private void Button_ClearAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < _playerColors.Length; i++)
            {
                _playerColors[i] = Ra2PaletteColor.FromArgb(0, 0, 0, 0);
                Button btn = StackPanel_Colors.Children[i] as Button;
                btn.Background = Ra2PaletteColorToBrush(_playerColors[i]);
            }
        }

        private void Button_Help_Click(object sender, RoutedEventArgs e)
        {
            ShowMessageBox("点击颜色方块后鼠标移到图片左键选择颜色 或 手动输入颜色");
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

        private void Button_EditColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Ra2PaletteColor selectedColor = Ra2PaletteColor.FromColor(colorDialog.Color);

                _playerColors[_selectIndex] = selectedColor;
                Button btn = StackPanel_Colors.Children[_selectIndex] as Button;
                btn.Background = Ra2PaletteColorToBrush(selectedColor);
            }
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            GData.PaletteConfig.PalettePlayerColor.Clear();

            foreach (var color in _playerColors)
            {
                GData.PaletteConfig.PalettePlayerColor.Add(color);
            }

            this.Close();
        }
    }
}
