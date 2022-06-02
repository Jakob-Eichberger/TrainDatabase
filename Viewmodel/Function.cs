using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Model;
using TrainDatabase.Z21Client;
using TrainDatabase.Z21Client.DTO;
using TrainDatabase.Z21Client.Enums;

namespace Viewmodel
{
    public class Function
    {
        public Function(IServiceProvider serviceProvider, FunctionModel functionModel)
        {
            ServiceProvider = serviceProvider;
            Db = ServiceProvider.GetService<Database>()!;
            Z21Client = ServiceProvider.GetService<Z21Client>()!;

            FunctionModel = Db.Functions.Include(e => e.Vehicle).FirstOrDefault(e => e.Id == functionModel.Id) ?? throw new ApplicationException($"Function '{functionModel}' was not found!");

            Z21Client.OnGetLocoInfo += Z21Client_OnGetLocoInfo;
        }

        /// <summary>
        /// Holds the state of the button. True if the Button is pressed, false if the Button is released.
        /// </summary>
        public bool State { get; private set; }

        private Database Db { get; }

        private Z21Client Z21Client { get; }

        private IServiceProvider ServiceProvider { get; }

        public event EventHandler<bool>? StateChanged;

        public FunctionModel FunctionModel { get; }

        private void Z21Client_OnGetLocoInfo(object? sender, TrainDatabase.Z21Client.Events.GetLocoInfoEventArgs e)
        {
            if (e.Data.Adresse.Value == FunctionModel.Vehicle.Address && e.Data.Functions.Any(e => e.address == FunctionModel.Address))
            {
                State = e.Data.Functions.First(e => e.address == FunctionModel.Address).state;

                StateChanged?.Invoke(this, State);
            }
        }

        /// <summary>
        /// Sets the state for this function.
        /// </summary>
        /// <param name="toggleType"></param>
        public void SetState(ToggleType toggleType)
        {
            if (FunctionModel.EnumType != FunctionType.None)
            {
                var functions = FunctionModel.Vehicle.TractionVehicleIds.Select(e => Db.Vehicles.Include(e => e.Functions).FirstOrDefault(f => f.Id == e)).SelectMany(e => e?.Functions ?? new List<FunctionModel>()).Where(e => e.EnumType == FunctionModel.EnumType && e.ButtonType == FunctionModel.ButtonType).ToList();
                functions.Add(FunctionModel);

                List<(ToggleType toggle, FunctionModel Func)> list = new();

                foreach (var item in functions)
                    list.Add((toggleType, item));

                Z21Client.SetLocoFunction(list);
            }
            else
            {
                Z21Client.SetLocoFunction(FunctionModel, toggleType);
            }
        }

        /// <summary>
        /// Sets the state for this function.
        /// </summary>
        /// <param name="state"></param>
        public void SetState(bool state) => SetState(state ? ToggleType.On : ToggleType.Off);
    }
}