﻿using System;
using System.Collections.Generic;

#nullable disable

namespace WPF_Application
{
    public partial class ControlStationNote
    {
        public long Id { get; set; }
        public long? PageId { get; set; }
        public long? X { get; set; }
        public long? Y { get; set; }
        public string Text { get; set; }
        public long? FontSize { get; set; }
        public long? Angle { get; set; }
        public long? Type { get; set; }
    }
}
