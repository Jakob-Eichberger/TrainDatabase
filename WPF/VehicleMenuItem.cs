using Model;
using System.Windows.Controls;

namespace Wpf_Application
{
    public class VehicleMenuItem : MenuItem
    {
        public Vehicle Vehicle { get; set; } = default!;
    }
}
