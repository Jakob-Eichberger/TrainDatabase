using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WPF_Application
{
    public class Theme
    {
        public Brush Primary { get; set; } = Brushes.White;

        public Brush Secondary { get; set; } = Brushes.White;

        public Brush TextColour { get; set; } = Brushes.Black;

    }
}
