using System;
using System.Collections.Generic;

#nullable disable

namespace WPF_Application
{
    public partial class ControlStationRouteList
    {
        public long Id { get; set; }
        public long? RouteId { get; set; }
        public string ControlId { get; set; }
        public long? StateId { get; set; }
        public long? Position { get; set; }
        public long? WaitTime { get; set; }
    }
}
