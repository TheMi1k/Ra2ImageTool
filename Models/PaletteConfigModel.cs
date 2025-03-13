using Ra2ImageTool.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ra2ImageTool.Models
{
    internal class PaletteConfigModel
    {
        public List<Ra2PaletteColor> PalettePlayerColor { get; set; } = new List<Ra2PaletteColor>();

        public Ra2PaletteColor[] PaletteHeaderColor { get; set; } = new Ra2PaletteColor[3];
    }
}
