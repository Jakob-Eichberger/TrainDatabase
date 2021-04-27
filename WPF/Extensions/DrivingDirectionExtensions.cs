using ModelTrainController.Z21;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Application.Extensions
{
    public static class DrivingDirectionExtensions
    {
        public static bool ConvertToBool(this DrivingDirection e) => e == DrivingDirection.F;
    }
}
