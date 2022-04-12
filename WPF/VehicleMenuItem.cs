using Model;
using System;
using System.Windows.Controls;

namespace Wpf_Application
{
    public class VehicleMenuItem : MenuItem
    {
        public VehicleMenuItem(Vehicle vehicle, string content, Action<Vehicle> onClick)
        {
            Vehicle = vehicle;
            Header = content;
            Click += (a, b) => onClick(Vehicle);
        }

        public Vehicle Vehicle { get; set; } = default!;
    }
}
