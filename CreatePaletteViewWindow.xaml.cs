using Ra2ImageTool.Data;
using Ra2ImageTool.Funcs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ra2ImageTool
{
    /// <summary>
    /// CreatePaletteViewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CreatePaletteViewWindow : Window
    {
        public bool Result { get; private set; } = false;

        public CreatePaletteViewWindow(List<Ra2PaletteColor> colorList)
        {
            InitializeComponent();
            Init(colorList);
        }

        private void Init(List<Ra2PaletteColor> colorList)
        {
            StackPanel_Color.Children.Clear();

            int index = 0;
            for (int j = 0; j < 8; j++)
            {
                StackPanel stackPanel = new StackPanel()
                {
                    Width = 28,
                    Height = 384,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Orientation = Orientation.Vertical,
                };

                for (int i = 0; i < 32; i++)
                {
                    stackPanel.Children.Add(new System.Windows.Shapes.Rectangle()
                    {
                        Width = 28,
                        Height = 12,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)Clamp(colorList[index].R * 4), (byte)Clamp(colorList[index].G * 4), (byte)Clamp(colorList[index].B * 4))),
                    });

                    index++;
                }

                StackPanel_Color.Children.Add(stackPanel);
            }
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            Result = true;

            this.Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = false;

            this.Close();
        }

        private static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }
    }
}
