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
        private ServiceProvider serviceProvider { get; }

        public App()
        {
            ServiceCollection services = new();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
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
                    serviceProvider.GetService<LogWindow>()!.Show();

                var client = serviceProvider.GetService<Client>() ?? throw new ApplicationException();
                client.LogMessage += (a, b) => Logger.LogInformation(b?.Message ?? "Error with no message");
                client.Connect(Configuration.ClientIP, Configuration.ClientPort, Configuration.GetBool("AllowNatTraversal") ?? true);

                serviceProvider.GetService<MainWindow>()!.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
