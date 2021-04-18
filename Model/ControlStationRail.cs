using System;
using System.Collections.Generic;

#nullable disable

namespace Wpf_Application
{
    public partial class ControlStationRail
    {
        public long Id { get; set; }
        public long? PageId { get; set; }
        public long? LeftControlId { get; set; }
        public long? RightControlId { get; set; }
        public long? LeftOutlet { get; set; }
        public long? RightOutlet { get; set; }
        public double? Value { get; set; }
        public long? LeftResponseModuleId { get; set; }
        public long? RightResponseModuleId { get; set; }
    }
}
