using System.ComponentModel.DataAnnotations.Schema;

namespace Service.ImportService.Z21.TDO
{
  [Table("functions")]
  internal class FunctionDTO
  {
    [Column("id")]
    internal long id { get; set; }

    [Column("vehicle_id")]
    internal long vehicle_id { get; set; }

    [Column("button_type")]
    internal long button_type { get; set; }

    [Column("shortcut")]
    internal string shortcut { get; set; }

    [Column("time")]
    internal string time { get; set; }

    [Column("position")]
    internal long position { get; set; }

    [Column("image_name")]
    internal string image_name { get; set; }

    [Column("function")]
    internal long function { get; set; }

    [Column("show_function_number")]
    internal long show_function_number { get; set; }

    [Column("is_configured")]
    internal long is_configured { get; set; }
  }
}