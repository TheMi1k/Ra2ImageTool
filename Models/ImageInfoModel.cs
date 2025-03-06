using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Drawing;
using Ra2ImageTool.Data;
using Ra2ImageTool.Funcs;

namespace Ra2ImageTool.Models
{
    public class ImageInfoModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal void SetInputDefault()
        {
            InImg = null;
            OutImg = null;

            Alpha = 0;
            Lightness = 0;
            TransparentDiffusion = 0.5;
            Name = "";
            IsTransparent = false;
            DistanceMatrix = null;

            IsChanged = false;

            GC.Collect();
        }

        internal void SetOverlayDefault()
        {
            ImgOverlay = null;

            NameOverlay = "";
            OverlayXOffset = 0;
            OverlayYOffset = 0;
            OverlayMode = GData.OverlayMode.叠加在上;

            GC.Collect();
        }

        internal void DisposeImage()
        {
            ImageManage.DisposeBitmap(InImg);
            ImageManage.DisposeBitmap(OutImg);

            InImg = null;
            OutImg = null;

            Alpha = 0;
            Lightness = 0;
            TransparentDiffusion = 0.5;
            Name = "";
            InImgPath = "";
            IsTransparent = false;
            DistanceMatrix = null;

            IsChanged = false;

            GC.Collect();
        }

        internal void DisposeImageOverlay()
        {
            ImageManage.DisposeBitmap(ImgOverlay);
            ImgOverlay = null;

            ImgOverlayPath = "";
            NameOverlay = "";
            OverlayXOffset = 0;
            OverlayYOffset = 0;
            OverlayMode = GData.OverlayMode.叠加在上;

            GC.Collect();
        }

        private void SetOverlayIsChanged()
        {
            if (OverlayXOffset != 0)
            {
                IsOverlayChanged = true;
                return;
            }
            if (OverlayYOffset != 0)
            {
                IsOverlayChanged = true;
                return;
            }
            if (OverlayMode != GData.OverlayMode.叠加在上)
            {
                IsOverlayChanged = true;
                return;
            }

            IsOverlayChanged = false;
        }

        private int _index { get; set; } = -1;

        [JsonIgnore]
        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                IndexStr = value.ToString().PadLeft(5, '0');
            }
        }

        [JsonIgnore]
        public string IndexStr { get; set; } = "00000";

        [JsonIgnore]
        public string Name { get; set; } = "";

        [JsonIgnore]
        public string NameOverlay { get; set; } = "";

        [JsonIgnore]
        public Bitmap InImg { get; set; } = null;

        [JsonIgnore]
        public Bitmap OutImg { get; set; } = null;

        [JsonIgnore]
        public Bitmap ImgOverlay { get; set; } = null;

        public string InImgPath { get; set; } = "";

        public string ImgOverlayPath { get; set; } = "";

        private int _overlayXOffset { get; set; } = 0;
        public int OverlayXOffset
        {
            get => _overlayXOffset;
            set
            {
                _overlayXOffset = value;
                SetOverlayIsChanged();
            }
        }

        private int _overlayYOffset { get; set; } = 0;
        public int OverlayYOffset
        {
            get => _overlayYOffset;
            set
            {
                _overlayYOffset = value;
                SetOverlayIsChanged();
            }
        }

        private bool _isChanged { get; set; } = false;

        public bool IsChanged
        {
            get => _isChanged;
            set
            {
                _isChanged = value;

                StrColor = _isChanged ? "#FF0000" : "#000000";

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StrColor"));
            }
        }

        private bool _isOverlayChanged { get; set; } = false;
        public bool IsOverlayChanged
        {
            get => _isOverlayChanged;
            set
            {
                _isOverlayChanged = value;

                OverlayStrColor = _isOverlayChanged ? "#FF0000" : "#000000";

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OverlayStrColor"));
            }
        }

        [JsonIgnore]
        public string StrColor { get; set; } = "#000000";

        [JsonIgnore]
        public string OverlayStrColor { get; set; } = "#000000";

        public int Alpha { get; set; } = 0;

        public double Lightness { get; set; } = 0;

        public double TransparentDiffusion { get; set; } = 0.5;

        /// <summary>
        /// 是否应用对图像轮廓透明过渡
        /// </summary>
        public bool IsTransparent { get; set; } = false;

        /// <summary>
        /// 过渡矩阵
        /// </summary>
        [JsonIgnore]
        public double[,] DistanceMatrix { get; set; } = null;

        private GData.OverlayMode _overlayMode { get; set; } = GData.OverlayMode.叠加在上;
        public GData.OverlayMode OverlayMode
        {
            get => _overlayMode;
            set
            {
                _overlayMode = value;
                SetOverlayIsChanged();
            }
        }
    }
}
