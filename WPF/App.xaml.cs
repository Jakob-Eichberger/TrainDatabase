using Helper;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Service;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using WPF_Application;
using Z21;

namespace Wpf_Application
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider ServiceProvider { get; }

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ServiceCollection services = new();
            ConfigureServices(services);
            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .WriteTo.File("Log\\log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                        .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug, theme: AnsiConsoleTheme.Sixteen)
                        .CreateLogger();

            ServiceProvider = services.BuildServiceProvider();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Log.Logger.Error($"{ex?.Message}");
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddDbContext<Database>();
            services.AddSingleton<Client>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<TrackPowerService>();
            services.AddSingleton<VehicleService>();
            services.AddSingleton<LogEventBus>();
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {

                using (var db = new Database())
                {
                    //db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                }

                if (Configuration.OpenDebugConsoleOnStart || Debugger.IsAttached)
                {
                    AllocConsole();
                }

                var client = ServiceProvider.GetService<Client>() ?? throw new ApplicationException();
                client.Connect(Configuration.ClientIP);
                ServiceProvider.GetService<MainWindow>()!.Show();
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, $"Failed to initialize the application!");
            }
        }

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();
    }
}
