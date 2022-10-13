using Helper;
using Microsoft.Extensions.DependencyInjection;
using Model;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TrainDatabase;
using Z21;

namespace WPF_Application.TimeCapture
{
    /// <summary>
    /// Interaction logic for MultiTraction.xaml
    /// </summary>
    public partial class MultiTraction : UserControl, INotifyPropertyChanged
    {
        public MultiTraction(IServiceProvider serviceProvider, VehicleModel? vehicleModel = null)
        {
            ServiceProvider = serviceProvider;
            VehicleModel = vehicleModel;
            LogService = ServiceProvider.GetService<LogEventBus>();
            InitializeComponent();

            if (VehicleModel is not null)
            {
                TimeCapture = new(ServiceProvider, VehicleModel);
                TimeCapture.StateChanged += (a, b) => Dispatcher.Invoke(async () =>
                {
                    await DrawSpeedDataPlot();
                    DrawSpeedMeasurementTable();
                    OnPropertyChanged();
                });
            }
            else
            {
                IsEnabled = false;
            }

            _ = DrawSpeedDataPlot();
            DrawSpeedMeasurementTable();

        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private IServiceProvider ServiceProvider { get; }

        public Viewmodel.TimeCapture? TimeCapture { get; }

        private VehicleModel? VehicleModel { get; }
        public LogEventBus LogService { get; private set; }

        protected void OnPropertyChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

        private async void BtnStartSpeedMeasurement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ArduinoSerialPort.PortAvailable(Configuration.ArduinoComPort))
                {
                    MessageBox.Show("Bitte gültigen ComPort angeben!", "Error", MessageBoxButton.OK);
                    return;
                }

                if (MessageBoxResult.OK == MessageBox.Show("CV 3 und 4 bitte manuel auf den Wert 0 stellen. Dann ok klicken.", "CV Wert setzen", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation))
                {
                    IsEnabled = false;
                    await TimeCapture?.Run()!;
                }
            }
            catch (Exception ex)
            {
                LogService.Log(Microsoft.Extensions.Logging.LogLevel.Error, ex);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void DrawSpeedMeasurementTable()
        {
            sp_Table.Children.Clear();
            Grid functionGrid = new()
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            };
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            CreatSpeedTableRow($"Step", $"km/h (V)", $"km/h (R)");
            bool lastStep = false;
            for (int i = Viewmodel.TimeCapture.StartMeasurement; i <= Client.maxDccStep; i += Viewmodel.TimeCapture.StepMeasurement)
            {
                string text2 = $"{(TimeCapture?.TractionForward[i] is null ? "-" : (double)Math.Round((TimeCapture.TractionForward[i] / 3.6m) ?? 0, 2))} km/h";
                string text3 = $"{(TimeCapture?.TractionBackward[i] is null ? "-" : (double)Math.Round((TimeCapture.TractionBackward[i] / 3.6m) ?? 0, 2))}  km/h";
                CreatSpeedTableRow($"Step {i}", text2, text3);

                if (!lastStep && i + Viewmodel.TimeCapture.StepMeasurement > Client.maxDccStep)
                {
                    i = (Client.maxDccStep - Viewmodel.TimeCapture.StepMeasurement);
                    lastStep = true;
                }
            }

            void CreatSpeedTableRow(string text1, string text2, string text3)
            {
                int row = functionGrid.RowDefinitions.Count;
                functionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Label step = new() { Content = text1, Margin = new Thickness(0, 0, 2, 1), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
                Grid.SetColumn(step, 0);
                Grid.SetRow(step, row);
                functionGrid.Children.Add(step);

                Label forward = new() { Content = text2, Margin = new Thickness(0, 0, 2, 1), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
                Grid.SetColumn(forward, 1);
                Grid.SetRow(forward, row);
                functionGrid.Children.Add(forward);

                Label backward = new() { Content = text3, Margin = new Thickness(0, 0, 2, 1), HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
                Grid.SetColumn(backward, 2);
                Grid.SetRow(backward, row);
                functionGrid.Children.Add(backward);
            }

            sp_Table.Children.Add(functionGrid);
        }

        private async Task DrawSpeedDataPlot()
        {
            if (sp_Plot is null) return;

            var pointsForward = await GetDataPointsList(TimeCapture?.TractionForward);
            var pointsBackward = await GetDataPointsList(TimeCapture?.TractionBackward);

            PlotModel model = new();

            OxyPlot.Wpf.PlotView plot = new()
            {
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
            };

            LineSeries linePoints_Forward = new() { StrokeThickness = 1, MarkerSize = 1, Title = "Vorwärts", Color = OxyPlot.OxyColors.Red };
            linePoints_Forward.Points.AddRange(pointsForward);

            LineSeries linePoints_Backwards = new() { StrokeThickness = 1, MarkerSize = 1, Title = "Rückwärts", Color = OxyPlot.OxyColors.Blue };
            linePoints_Backwards.Points.AddRange(pointsBackward);

            model.Axes.Add(new LinearAxis()
            {
                Maximum = Client.maxDccStep,
                Minimum = Viewmodel.TimeCapture.StartMeasurement,
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Title = "Dcc Speed Step"
            });

            model.Axes.Add(new LinearAxis()
            {
                Minimum = 0,
                MinorStep = double.NaN,
                Title = "km/h"
            });

            model.Series.Add(linePoints_Backwards);
            model.Series.Add(linePoints_Forward);

            plot.Model = model;
            //plot.InvalidatePlot(true);
            sp_Plot.Children.Clear();
            sp_Plot.Children.Add(plot);
        }

        private async Task<List<DataPoint>> GetDataPointsList(decimal?[]? values) => await Task.Run(() =>
        {
            List<DataPoint> pointsBackward = new();

            for (int i = 2; i <= Client.maxDccStep; i++)
            {
                if (values?[i] is not null)
                    pointsBackward.Add(new(i, (double)Math.Round((values[i] / 3.6m) ?? 0, 2)));
            }

            return pointsBackward;
        });

    }
}
