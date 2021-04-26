using Model;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool ValueChanged { get; set; }

        public string ControllerIp
        {
            get => Settings.ControllerIP.ToString();
            set
            {
                try
                {
                    ValueChanged = value != ControllerIp;
                    Settings.ControllerIP = IPAddress.Parse(value);
                }
                catch
                {
                    ValueChanged = false;
                    MessageBox.Show($"'{value}' ist keine valide Ip!");
                }
            }
        }

        public bool UsingJoyStick
        {
            get
            {
                return Settings.UsingJoyStick;
            }
            set
            {
                ValueChanged = UsingJoyStick != value;
                Settings.UsingJoyStick = value;
                OnPropertyChanged();
            }
        }

        public SettingsWindow()
        {
            InitializeComponent();
            DrawLokoFunctionEnumButtons();
        }

        private void DrawLokoFunctionEnumButtons()
        {
            SPLocoFunctions.Children.Clear();
            var JoyStickFunctionDictionary = Settings.GetJoyStickFunctionDictionary();
            Grid functionTypeGrid = new();
            functionTypeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionTypeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionTypeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            foreach (var item in Enum.GetValues(typeof(FunctionType)))
            {
                if ((FunctionType)item != 0)
                {
                    functionTypeGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    Label enumLabel = new() { Content = Enum.GetName((FunctionType)item), Margin = new Thickness(0, 0, 20, 5) };
                    Grid.SetColumn(enumLabel, 0);
                    Grid.SetRow(enumLabel, functionTypeGrid.RowDefinitions.Count - 1);
                    functionTypeGrid.Children.Add(enumLabel);

                    Label currentJoystickOffset = new() { Content = JoyStickFunctionDictionary.TryGetValue((FunctionType)item, out var v) ? Enum.GetName(v.joyStick) : "-", Margin = new Thickness(0, 0, 20, 5) };
                    Grid.SetColumn(currentJoystickOffset, 1);
                    Grid.SetRow(currentJoystickOffset, functionTypeGrid.RowDefinitions.Count - 1);
                    functionTypeGrid.Children.Add(currentJoystickOffset);

                    Button button = new() { Content = "Neu belegen.", Padding = new(5), Margin = new Thickness(0, 0, 20, 5), Tag = (FunctionType)item };
                    button.Click += ButtonNeuBelegen_Click;
                    Grid.SetColumn(button, 2);
                    Grid.SetRow(button, functionTypeGrid.RowDefinitions.Count - 1);
                    functionTypeGrid.Children.Add(button);
                }
            }
            SPLocoFunctions.Children.Add(functionTypeGrid);
        }

        private void ButtonNeuBelegen_Click(object? sender, EventArgs s)
        {
            MessageBox.Show(Enum.GetName((FunctionType)((sender as Button).Tag)));
        }

        private void settingsw_Closed(object sender, EventArgs e)
        {
            if (ValueChanged)
            {
                MessageBox.Show("Geänderte Einstellungen werden teilweise erst nach einem Restart übernommen!", "Geänderte Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
