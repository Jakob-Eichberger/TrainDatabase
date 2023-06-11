using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_Application.TrainControl.FunctionButton;

namespace WPF_Application.TrainControl
{
    /// <summary>
    /// Interaction logic for FunctionControl.xaml
    /// </summary>
    public partial class FunctionControl : UserControl
    {
        public FunctionControl(IServiceProvider serviceProvider, VehicleModel vehicle)
        {
            InitializeComponent();

            ServiceProvider = serviceProvider;
            Vehicle = vehicle;
            Db = ServiceProvider.GetService<Database>()!;
            Db.ChangeTracker.StateChanged += (a, b) => DrawAllFunctions();
            DrawAllFunctions();
        }

        private IServiceProvider ServiceProvider { get; }

        private VehicleModel Vehicle { get; }

        private Database Db { get; }

        /// <summary>
        /// Functions draws every single Function of a vehicle for the user to click on. 
        /// </summary>
        private void DrawAllFunctions() => Dispatcher.BeginInvoke(() =>
        {
            FunctionGrid.Children.Clear();
            foreach (var item in Vehicle.Functions.OrderBy(e => e.Address))
            {
                switch (item.ButtonType)
                {
                    case ButtonType.Switch:
                        FunctionGrid.Children.Add(new SwitchButton(ServiceProvider, item));
                        break;
                    case ButtonType.PushButton:
                        FunctionGrid.Children.Add(new PushButton(ServiceProvider, item));
                        break;
                    case ButtonType.Timer:
                        FunctionGrid.Children.Add(new TimerButton(ServiceProvider, item));
                        break;
                }
            }
        });
    }
}
