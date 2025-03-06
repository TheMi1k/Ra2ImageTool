using System.ComponentModel;
using System.Windows;
using Ra2ImageTool.Data;

namespace Ra2ImageTool.Models
{
    public class UIDataModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal void ChangeOverlayMargin(int x, int y)
        {
            _isChangeOverlayMargin = false;

            OverlayXOffsetStr = x.ToString();
            OverlayYOffsetStr = y.ToString();

            _isChangeOverlayMargin = true;

            OverlayGridThickness = new Thickness(x, y, 0, 0);
        }

        internal void SetOutImageData(int index = -1)
        {
            if (index == -1)
            {
                index = NowIndex;
            }
            GData.ListViewData[index].TransparentDiffusion = TransparentDiffusion;
            GData.ListViewData[index].Lightness = Lightness;
            GData.ListViewData[index].Alpha = Alpha;
            GData.ListViewData[index].IsTransparent = IsTransparent;
        }

        internal void SetOverlayData(int index = -1)
        {
            if (index == -1)
            {
                index = NowIndex;
            }

            if (GData.ListViewData.Count < index + 1)
            {
                return;
            }

            GData.ListViewData[index].OverlayXOffset = OverlayXOffset;
            GData.ListViewData[index].OverlayYOffset = OverlayYOffset;
            GData.ListViewData[index].OverlayMode = OverlayMode;
        }

        internal void SetAllDefault()
        {
            _isChangeOverlayMargin = false;

            NowIndex = 0;
            TransparentDiffusion = 0.5;
            Lightness = 0.0;
            Alpha = 0;
            OutlineTransparentOffset = 255;
            OverlayXOffset = 0;
            OverlayYOffset = 0;
            OverlayXOffsetStr = "0";
            OverlayYOffsetStr = "0";
            OverlayGridThickness = new Thickness(0, 0, 0, 0);
            OverlayMode = GData.OverlayMode.叠加在上;
            IsTransparent = false;

            _isChangeOverlayMargin = true;
        }

        internal void SetProgressUI(int suc, int max)
        {
            ProgressUi = $"{suc} / {max}";
        }

        private string _progressUi { get; set; } = "0 / 0";
        public string ProgressUi
        {
            get => _progressUi;
            private set
            {
                _progressUi = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressUi"));
            }
        }

        private int _nowIndex { get; set; } = 0;
        public int NowIndex
        {
            get => _nowIndex;

            set
            {
                _nowIndex = value;
                IndexStr = _nowIndex.ToString().PadLeft(5, '0');

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NowIndex"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IndexStr"));
            }
        }

        public string IndexStr { get; set; } = "00000";

        private double _transparentDiffusionStr { get; set; } = 0.5;
        public double TransparentDiffusion
        {
            get => _transparentDiffusionStr;
            set
            {
                _transparentDiffusionStr = value;
                TransparentDiffusionStr = _transparentDiffusionStr.ToString("f2");

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TransparentDiffusion"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TransparentDiffusionStr"));
            }
        }

        public string TransparentDiffusionStr { get; set; } = "0.5";

        private double _lightness { get; set; } = 0.0;
        public double Lightness
        {
            get => _lightness;
            set
            {
                _lightness = value;
                LightnessStr = _lightness.ToString("f2");

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Lightness"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LightnessStr"));
            }
        }

        public string LightnessStr { get; set; } = "0.0";

        private int _alpha { get; set; } = 0;
        public int Alpha
        {
            get => _alpha;
            set
            {
                _alpha = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Alpha"));
            }
        }

        private int _outlineTransparentOffset { get; set; } = 255;
        public int OutlineTransparentOffset
        {
            get => _outlineTransparentOffset;
            set
            {
                _outlineTransparentOffset = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OutlineTransparentOffset"));
            }
        }

        private bool _isTransparent { get; set; } = false;
        public bool IsTransparent
        {
            get => _isTransparent;
            set
            {
                _isTransparent = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsTransparent"));
            }
        }

        public int OverlayXOffset { get; private set; } = 0;
        public int OverlayYOffset { get; private set; } = 0;

        private bool _isChangeOverlayMargin { get; set; } = true;

        private string _overlayXOffsetStr { get; set; } = "0";
        public string OverlayXOffsetStr
        {
            get => _overlayXOffsetStr;
            set
            {
                _overlayXOffsetStr = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OverlayXOffsetStr"));

                if (value == "")
                {
                    OverlayXOffset = 0;
                }
                else if (int.TryParse(value, out int x))
                {
                    OverlayXOffset = x;
                }
                else
                {
                    return;
                }

                if (_isChangeOverlayMargin)
                {
                    OverlayGridThickness = new Thickness(OverlayXOffset, OverlayYOffset, 0, 0);
                    SetOverlayData();
                }
            }
        }

        private string _overlayYOffsetStr { get; set; } = "0";
        public string OverlayYOffsetStr
        {
            get => _overlayYOffsetStr;
            set
            {
                _overlayYOffsetStr = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OverlayYOffsetStr"));

                if (value == "")
                {
                    OverlayYOffset = 0;
                }
                else if (int.TryParse(value, out int y))
                {
                    OverlayYOffset = y;
                }
                else
                {
                    return;
                }

                if (_isChangeOverlayMargin)
                {
                    OverlayGridThickness = new Thickness(OverlayXOffset, OverlayYOffset, 0, 0);
                    SetOverlayData();
                }
            }
        }


        private Thickness _overlayGridThickness { get; set; } = new Thickness(0, 0, 0, 0);
        public Thickness OverlayGridThickness
        {
            get=> _overlayGridThickness;
            private set
            {
                _overlayGridThickness = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OverlayGridThickness"));
            }
        }

        private GData.OverlayMode _overlayMode { get; set; } = GData.OverlayMode.叠加在上;
        public GData.OverlayMode OverlayMode
        {
            get => _overlayMode;
            set
            {
                _overlayMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OverlayMode"));
            }
        }
    }
}
