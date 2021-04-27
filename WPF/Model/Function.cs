using System;
using System.Collections.Generic;

#nullable disable

namespace Model
{
    public partial class Function
    {
        public int Id { get; set; }
        public Vehicle Vehicle { get; set; }
        public int VehicleId { get; set; }
        public int ButtonType { get; set; }
        public string Shortcut { get; set; }
        public decimal Time { get; set; }
        public int Position { get; set; }
        public string ImageName { get; set; }
        public int FunctionIndex { get; set; }
        public bool ShowFunctionNumber { get; set; }
        public bool IsConfigured { get; set; }
        public FunctionType Type { get; set; }
    }
}
