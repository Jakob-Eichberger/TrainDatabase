using System;
using System.Collections.Generic;

#nullable disable

namespace WPF_Application
{
    public partial class DcFunction
    {
        public long Id { get; set; }
        public long? VehicleId { get; set; }
        public long? Position { get; set; }
        public string Time { get; set; }
        public string ImageName { get; set; }
        public long? Function { get; set; }
        public string CabFunctionDescription { get; set; }
        public string DriversCab { get; set; }
        public string Shortcut { get; set; }
        public long ButtonType { get; set; }
        public long ShowFunctionNumber { get; set; }
        public long IsConfigured { get; set; }
    }
}
