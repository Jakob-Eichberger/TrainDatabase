using System;
using System.Collections.Generic;

#nullable disable

namespace Model
{
    public partial class Function
    {
        public long Id { get; set; }
        public long? VehicleId { get; set; }
        public long ButtonType { get; set; }
        public string Shortcut { get; set; }
        public string Time { get; set; }
        public long? Position { get; set; }
        public string ImageName { get; set; }
        public long? FunctionIndex { get; set; }
        public long ShowFunctionNumber { get; set; }
        public long IsConfigured { get; set; }
    }
}
