using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using TrainDatabase;
using TrainDatabase.Z21Client;
using WPF_Application;

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
            services.AddSingleton<Z21Client>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<LogWindow>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                if (Configuration.OpenDebugConsoleOnStart)
                    serviceProvider.GetService<LogWindow>()!.Show();

                serviceProvider.GetService<MainWindow>()!.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application failed to start. {ex.Message}");
            }
        }
    }
}
