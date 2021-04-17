using System;
using System.Collections.Generic;

#nullable disable

namespace WPF_Application
{
    public partial class TractionList
    {
        public long Id { get; set; }
        public long? LocoId { get; set; }
        public long? RegulationStep { get; set; }
        public double? Time { get; set; }
    }
}
