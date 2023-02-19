using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service.ImportService.Z21.TDO
{
    internal class VehicleDTO
    {
        internal long id { get; set; }

        internal string name { get; set; }

        internal string image_name { get; set; }

        internal long type { get; set; }

        internal long max_speed { get; set; }

        internal long address { get; set; }

        internal long active { get; set; }

        internal long position { get; set; }

        internal string full_name { get; set; }

        internal long speed_display { get; set; }

        internal string railway { get; set; }

        internal long traction_direction { get; set; }

        internal string description { get; set; }

        internal long dummy { get; set; }
    }
}
