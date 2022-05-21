using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewmodel
{
    internal class Vehicle
    {
        public Vehicle(IServiceProvider serviceProvider, VehicleModel vehicleModel, Action? StateChanged = null)
        {
            ServiceProvider = serviceProvider;
            VehicleModel = vehicleModel;
            this.StateChanged = StateChanged;
        }

        public IServiceProvider ServiceProvider { get; }
        public VehicleModel VehicleModel { get; }
        public Action? StateChanged { get; }
    }
}
