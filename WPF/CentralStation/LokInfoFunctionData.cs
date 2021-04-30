using Helper;
using ModelTrainController.Z21;

namespace ModelTrainController
{
    public class LokInfoFunctionData
    {
        public LokAdresse LokAdresse { get; set; } = default!;

        public ToggleType ToggleType { get; set; }

        public int FunctionIndex { get; set; }
    }
}