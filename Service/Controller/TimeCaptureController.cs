using Helper;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z21;

namespace Service.Controller
{
  public class TimeCaptureController
  {
    public TimeCaptureController(IServiceProvider serviceProvider, VehicleModel vehicleModel)
    {
      ServiceProvider = serviceProvider;
      VehicleModel = vehicleModel;
      Z21Client = ServiceProvider.GetService<Client>()!;
      Database = ServiceProvider.GetService<Database>()!;
      LogService = ServiceProvider.GetService<LogEventBus>()!;
      Vehicle = new(ServiceProvider, vehicleModel);
    }

    /// <summary>
    /// Occurs when a property changes.
    /// </summary>
    public event EventHandler? StateChanged;

    public event EventHandler? TimeCaptureFinished;

    public static decimal DistanceBetweenSensorsInMM
    {
      get => Configuration.GetDecimal(nameof(DistanceBetweenSensorsInMM)) ?? 200.0m;
      set => Configuration.Set(nameof(DistanceBetweenSensorsInMM), value.ToString());
    }

    public static int StartMeasurement
    {
      get => Configuration.GetInt(nameof(StartMeasurement)) ?? 2;
      set => Configuration.Set(nameof(StartMeasurement), value.ToString());
    }

    public static int StepMeasurement
    {
      get => Configuration.GetInt(nameof(StepMeasurement)) ?? 1;
      set => Configuration.Set(nameof(StepMeasurement), value.ToString());
    }

    public decimal?[] TractionBackward { get; private set; } = new decimal?[Client.maxDccStep + 1];

    public decimal?[] TractionForward { get; private set; } = new decimal?[Client.maxDccStep + 1];

    private Database Database { get; } = default!;

    private LogEventBus LogService { get; } = default!;

    private IServiceProvider ServiceProvider { get; }

    private VehicleController Vehicle { get; } = default!;

    private VehicleModel VehicleModel { get; }

    private Client Z21Client { get; } = default!;

    public bool IsRunning { get; private set; } = false;

    public async Task Run()
    {
      await Task.Run(
                     async () =>
                     {
                       try
                       {
                         if (!Z21Client.ClientReachable)
                         {
                           throw new ApplicationException(
                                                          "Z21 not connected. Please ensure that the z21 is turned on and in the same network as this pc. ");
                         }

                         IsRunning = true;

                         TractionBackward = new decimal?[Client.maxDccStep + 1];
                         TractionForward = new decimal?[Client.maxDccStep + 1];

                         Z21Client.SetTrackPowerON();

                         bool lastStep = false;

                         int stepMeasurement = StepMeasurement;
                         for (int speed = StartMeasurement;
                              speed <= Client.maxDccStep;
                              speed += stepMeasurement)
                         {
                           await CaptureTime(speed, true);
                           await CaptureTime(speed, false);

                           if (!lastStep && speed + stepMeasurement > Client.maxDccStep)
                           {
                             speed = Client.maxDccStep - stepMeasurement;
                             lastStep = true;
                           }
                         }

                         await ReturnHome();
                         await SaveChanges();

                         TimeCaptureFinished?.Invoke(this, null!);
                       }
                       finally
                       {
                         IsRunning = false;
                         OnStateChanged();
                       }
                     });
    }

    private async Task CaptureTime(int steps, bool direction)
    {
      decimal time = await GetTimeBetweenSensors(steps, direction) / 1000.0m;
      decimal speed = Math.Round(DistanceBetweenSensorsInMM / 1000.0m / time * 87.0m, 2);
      SetTractionSpeed(steps, direction, speed);
    }

    private async Task<decimal> GetTimeBetweenSensors(int speed, bool direction)
    {
      try
      {
        using ArduinoSerialPort port = new(Configuration.ArduinoComPort, Configuration.ArduinoBaudrate);

        Vehicle.SetDirection(direction);
        Vehicle.Speed = speed;

        string? data = await port.WaitForValue(int.Parse($"{new TimeSpan(0, 5, 0).TotalMilliseconds}"));

        return decimal.TryParse(
                                data.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture,
                                out decimal result)
                 ? result
                 : throw new ApplicationException($"Serial bus data ('{data}') could not be parsed as decimal!");
      }
      finally
      {
        await Task.Delay((int)new TimeSpan(0, 0, 2).TotalMilliseconds);
        Vehicle.Speed = 0;
      }
    }

    /// <summary>
    /// Raises the <see cref="StateChanged"/> event. 
    /// </summary>
    private void OnStateChanged()
    {
      StateChanged?.Invoke(this, null!);
    }

    private async Task ReturnHome()
    {
      using ArduinoSerialPort port = new(Configuration.ArduinoComPort);
      Vehicle.SetDirection(true);
      Vehicle.Speed = 40;
      await port.WaitForValue(int.Parse($"{new TimeSpan(0, 5, 0).TotalMilliseconds}"));
      Vehicle.SetDirection(false);
      await port.WaitForValue(int.Parse($"{new TimeSpan(0, 5, 0).TotalMilliseconds}"));
      Vehicle.SetDirection(true);
      Vehicle.Speed = 0;
    }

    private async Task SaveChanges()
    {
      VehicleModel? temp = Database.Vehicles.FirstOrDefault(e => e.Id == VehicleModel.Id) ??
                           throw new($"Fahrzeug mit Adresse '{VehicleModel.Address}' nicht gefunden!");
      temp.TractionBackward = TractionBackward;
      temp.TractionForward = TractionForward;
      await Database.SaveChangesAsync();
    }

    private void SetTractionSpeed(int speedStep, bool direction, decimal speed)
    {
      if (direction)
      {
        TractionForward[speedStep] = speed;
      }
      else
      {
        TractionBackward[speedStep] = speed;
      }

      LogService.Log(
                     Microsoft.Extensions.Logging.LogLevel.Information,
                     $"Loco drove {speed} m/s at dcc speed {speedStep} and direction {direction}.");
      OnStateChanged();
    }
  }
}