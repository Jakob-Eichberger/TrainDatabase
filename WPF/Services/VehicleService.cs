using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_Application.Infrastructure;

namespace WPF_Application.Services
{
    class VehicleService : BaseService
    {
        public VehicleService(Database db) : base(db)
        {
        }

        public void Update(Vehicle vehicle)
        {
            base.Update<Vehicle>(vehicle);
        }
    }
}
