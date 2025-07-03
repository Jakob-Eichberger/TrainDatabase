using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Z21;
using Z21.Model;
using Z21.Enums;
using Z21.Events;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Service.Controller
{
  public class FunctionController
  {
    public FunctionController(IServiceProvider serviceProvider, FunctionModel functionModel)
    {
      ServiceProvider = serviceProvider;
      Db = ServiceProvider.GetService<Database>()!;
      Client = ServiceProvider.GetService<Client>()!;

      FunctionModel = Db.Functions.Include(e => e.Vehicle).FirstOrDefault(e => e.Id == functionModel.Id) ??
                      throw new ApplicationException($"Function '{functionModel}' was not found!");

      Client.OnGetLocoInfo += Z21Client_OnGetLocoInfo;
    }

    /// <summary>
    /// Holds the state of the button. True if the Button is pressed, false if the Button is released.
    /// </summary>
    public bool State { get; private set; }

    private Database Db { get; }

    private Client Client { get; }

    private IServiceProvider ServiceProvider { get; }

    public event EventHandler<bool>? StateChanged;

    public FunctionModel FunctionModel { get; }

    private void Z21Client_OnGetLocoInfo(object? sender, GetLocoInfoEventArgs e)
    {
      if (e.Data.Adresse.Value == FunctionModel.Vehicle.Address &&
          e.Data.Functions.Any(e => e.address == FunctionModel.Address))
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
        List<FunctionModel> functions = FunctionModel.Vehicle.TractionVehicleIds
                                                     .Select(
                                                             e => Db.Vehicles.Include(e => e.Functions)
                                                                    .FirstOrDefault(f => f.Id == e))
                                                     .SelectMany(e => e?.Functions ?? new List<FunctionModel>())
                                                     .Where(
                                                            e => e.EnumType == FunctionModel.EnumType &&
                                                                 e.ButtonType == FunctionModel.ButtonType).ToList();
        functions.Add(FunctionModel);

        List<FunctionData> list = functions.Select(e => new FunctionData(e.Vehicle.Address, e.Address, toggleType))
                                           .ToList();

        Client.SetLocoFunction(list);
      }
      else
      {
        Client.SetLocoFunction(new FunctionData(FunctionModel.Vehicle.Address, FunctionModel.Address, toggleType));
      }
    }

    /// <summary>
    /// Sets the state for this function.
    /// </summary>
    /// <param name="state"></param>
    public void SetState(bool state)
    {
      SetState(state ? ToggleType.On : ToggleType.Off);
    }
  }
}