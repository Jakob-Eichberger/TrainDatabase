using ModelTrainController.Z21;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
    public static class BoolExtensions
    {
        public static DrivingDirection GetDrivingDirection(this bool e) => e ? DrivingDirection.F : DrivingDirection.R;
    }
}
