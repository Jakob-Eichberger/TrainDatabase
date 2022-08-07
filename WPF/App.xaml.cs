using Helper;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Service;
using System;
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
            ServiceCollection services = new();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddDbContext<Database>();
            services.AddSingleton<Client>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<TrackPowerService>();
            services.AddSingleton<LogWindow>();
            services.AddSingleton<VehicleService>();
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

                if (Configuration.OpenDebugConsoleOnStart)
                    ServiceProvider.GetService<LogWindow>()!.Show();

                var client = ServiceProvider.GetService<Client>() ?? throw new ApplicationException();
                client.LogMessage += (a, b) => Logger.LogInformation(b?.Message ?? "Error with no message");
                client.Connect(Configuration.ClientIP);

                ServiceProvider.GetService<MainWindow>()!.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
