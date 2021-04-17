using System;
using System.Collections.Generic;

#nullable disable

namespace WPF_Application
{
    public partial class TrainList
    {
        public long Id { get; set; }
        public long? TrainId { get; set; }
        public long? VehicleId { get; set; }
        public long? Position { get; set; }
    }
}
