using System;
using System.Collections.Generic;

#nullable disable

namespace WPF_Application
{
    public partial class ControlStationRoute
    {
        public long Id { get; set; }
        public long? PageId { get; set; }
        public string Name { get; set; }
        public long? X { get; set; }
        public long? Y { get; set; }
        public long? Angle { get; set; }
    }
}
