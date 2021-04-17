using System;
using System.Collections.Generic;

#nullable disable

namespace WPF_Application
{
    public partial class ControlStationControlState
    {
        public long Id { get; set; }
        public long? ControlId { get; set; }
        public long? State { get; set; }
        public long? Address1Value { get; set; }
        public long? Address2Value { get; set; }
        public long? Address3Value { get; set; }
    }
}
