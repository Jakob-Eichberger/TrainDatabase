using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service.ImportService.Z21.TDO
{
    [Table("functions")]
    internal class FunctionDTO
    {
        [Column("id")]
        internal long Id { get; set; }

        [Column("vehicle_id")]
        internal long VehicleId { get; set; }

        [Column("button_type")]
        internal long ButtonType { get; set; }

        [Column("shortcut")]
        internal string Shortcut { get; set; }

        [Column("time")]
        internal string Time { get; set; }

        [Column("position")]
        internal long Position { get; set; }

        [Column("image_name")]
        internal string ImageName { get; set; }

        [Column("function")]
        internal long function { get; set; }

        [Column("show_function_number")]
        internal long ShowFunctionNumber { get; set; }

        [Column("is_configured")]
        internal long IsConfigured { get; set; }
    }
}
