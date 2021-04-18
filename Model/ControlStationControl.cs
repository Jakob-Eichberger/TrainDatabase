using System;
using System.Collections.Generic;

#nullable disable

namespace Wpf_Application
{
    public partial class ControlStationControl
    {
        public long Id { get; set; }
        public long? PageId { get; set; }
        public long? X { get; set; }
        public long? Y { get; set; }
        public double? Angle { get; set; }
        public long? Type { get; set; }
        public long? Address1 { get; set; }
        public long? Address2 { get; set; }
        public long? Address3 { get; set; }
        public long? ButtonType { get; set; }
        public double? Time { get; set; }
    }
}
