using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using Helper;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Service;
using Wpf_Application;
using Z21;

namespace WPF_Application
{
  // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
  public class TrainDatabaseServiceProvider()
  {
    private const string KeyWord = "TrainDatabase";

    public static AutofacServiceProvider CreateServiceProvider()
    {
      return CreateServiceProvider<TrainDatabaseServiceProvider>();
    }

    public static AutofacServiceProvider CreateServiceProvider<T>() where T : TrainDatabaseServiceProvider
    {
      return Activator.CreateInstance<T>().Build();
    }

    protected virtual void ConfigureServices(ServiceCollection services)
    {
      services.AddDbContext<Database>();
      services.AddSingleton<Client>();
      services.AddSingleton<MainWindow>();
      services.AddSingleton<TrackPowerService>();
      services.AddSingleton<VehicleService>();
      services.AddSingleton<LogEventBus>();
      services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
    }

    protected virtual void CreateDatabase(AutofacServiceProvider serviceProvider)
    {
      Database? db = serviceProvider.GetRequiredService<Database>();
      db.Database.Migrate();
    }

    private AutofacServiceProvider Build()
    {
      ServiceCollection serviceCollection = new();
      ConfigureServices(serviceCollection);

      ContainerBuilder builder = new();
      builder.Populate(serviceCollection);
      Assembly[] assemblies = GetAllAssemblies();
      Log.Logger.Debug(
                       $"Register modules for assemblies: {string.Join(Environment.NewLine, assemblies.Select(e => e.FullName))}");
      builder.RegisterAssemblyModules(assemblies);
      IContainer container = builder.Build();
      AutofacServiceLocator serviceLocator = new(container);
      ServiceLocator.SetLocatorProvider(() => serviceLocator);
      AutofacServiceProvider serviceProvider = new(container);
      CreateDatabase(serviceProvider);
      return serviceProvider;
    }

    private Assembly[] GetAllAssemblies()
    {
      return AppDomain.CurrentDomain.GetAssemblies()
                      .Where(assembly => assembly.FullName?.StartsWith(KeyWord) == true).ToArray();
    }
  }
}