using System;
using System.Collections.Generic;

#nullable disable

namespace Model
{

    public enum ButtonType
    {
        Switch = 0,
        PushButton = 1,
        Timer = 2
    }
    public partial class Function
    {
        public int Id { get; set; }
        public Vehicle Vehicle { get; set; }
        public int VehicleId { get; set; }
        public ButtonType ButtonType { get; set; }
        public string Name { get; set; }
        public decimal Time { get; set; }
        public int Position { get; set; }
        public string ImageName { get; set; }
        public int FunctionIndex { get; set; }
        public bool ShowFunctionNumber { get; set; }
        public bool IsConfigured { get; set; }
        public FunctionType EnumType { get; set; }
    }
}
