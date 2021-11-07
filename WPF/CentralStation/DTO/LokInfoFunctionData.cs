using WPF_Application.CentralStation.Enum;

namespace WPF_Application.CentralStation.DTO
{
    public class LokInfoFunctionData
    {
        public int FunctionIndex { get; set; }

        public LokAdresse LokAdresse { get; set; } = default!;

        public ToggleType ToggleType { get; set; }
    }
}