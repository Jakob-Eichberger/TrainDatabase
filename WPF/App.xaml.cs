using Helper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Service;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Autofac.Extensions.DependencyInjection;
using WPF_Application;
using Z21;

namespace Wpf_Application
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private AutofacServiceProvider ServiceProvider { get; set; }

    public App()
    {
      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

      string logFilePath = Path.Combine(Configuration.ApplicationData.LogDirectory.FullName, "log.txt");
      Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Debug()
                  .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                  .Enrich.FromLogContext()
                  .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                  .WriteTo.Console(
                                   LogEventLevel.Debug,
                                   theme: AnsiConsoleTheme.Sixteen)
                  .CreateLogger();
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      Exception? ex = e.ExceptionObject as Exception;
      Log.Logger.Error($"{ex?.Message}");
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
      try
      {
        ServiceProvider = TrainDatabaseServiceProvider.CreateServiceProvider();

        if (Configuration.OpenDebugConsoleOnStart || Debugger.IsAttached)
        {
          AllocConsole();
        }

        Client client = ServiceProvider.GetService<Client>() ?? throw new ApplicationException();
        client.Connect(Configuration.ClientIP);
        ServiceProvider.GetService<MainWindow>()!.Show();
      }
      catch (Exception ex)
      {
        Log.Logger.Fatal(ex, $"Failed to initialize the application!");
        MessageBox.Show(ex.Message, "Fatal error");
        Environment.Exit(1);
      }
    }

    [DllImport("Kernel32")]
    public static extern void AllocConsole();

    [DllImport("Kernel32")]
    public static extern void FreeConsole();
  }
}