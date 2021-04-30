using Model;
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
