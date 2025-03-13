using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ra2ImageTool.Models
{
    public class ProjectModel
    {
        public double SoftwareVersion { get; set; } = -1;

        public List<ImageInfoModel> ImageInfo { get; set; }

        public List<string> PalettePlayerColor { get; set; }

        public List<string> PaletteHeaderColor { get; set; }

        public int PaletteColorNum { get; set; }
    }
}
