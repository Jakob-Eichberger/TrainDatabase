using TrainDatabase.Z21Client.Enum;

namespace TrainDatabase.Z21Client.DTO
{
    public class LokInfoFunctionData
    {
        public int FunctionIndex { get; set; }

        public LokAdresse LokAdresse { get; set; } = default!;

        public ToggleType ToggleType { get; set; }
    }
}