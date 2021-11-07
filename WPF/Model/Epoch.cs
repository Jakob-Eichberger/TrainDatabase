using System.ComponentModel;
using TrainDatabase;

namespace Model
{

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Epoch
    {
        [Description("Keine Epoche")]
        @default = 0,
        [Description("Epoche 1")]
        Epoch1 = 1,
        [Description("Epoche 2")]
        Epoch2 = 2,
        [Description("Epoche 3")]
        Epoch3 = 3,
        [Description("Epoche 4")]
        Epoch4 = 4,
        [Description("Epoche 5")]
        Epoch5 = 5,
        [Description("Epoche 6")]
        Epoch6 = 6
    }
}
