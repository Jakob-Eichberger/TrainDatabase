using System;
using System.Collections.Generic;

#nullable disable

namespace WPF_Application
{
    public partial class ControlStationResponseModule
    {
        public long Id { get; set; }
        public long? PageId { get; set; }
        public long? Type { get; set; }
        public long? Address { get; set; }
        public long? ReportAddress { get; set; }
        public long? Afterglow { get; set; }
        public long? X { get; set; }
        public long? Y { get; set; }
        public long? Angle { get; set; }
    }
}
