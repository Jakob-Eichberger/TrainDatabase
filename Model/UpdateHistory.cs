using System;
using System.Collections.Generic;

#nullable disable

namespace Wpf_Application
{
    public partial class UpdateHistory
    {
        public long Id { get; set; }
        public string Os { get; set; }
        public string UpdateDate { get; set; }
        public string BuildVersion { get; set; }
        public long? BuildNumber { get; set; }
        public long? ToDatabaseVersion { get; set; }
    }
}
