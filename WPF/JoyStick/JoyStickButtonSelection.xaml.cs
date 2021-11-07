using SharpDX.DirectInput;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TrainDatabase.JoyStick
{
    /// <summary>
    /// Interaction logic for JoyStickButtonSelection.xaml
    /// </summary>
    public partial class JoyStickButtonSelection : Window, INotifyPropertyChanged
    {
        private int maxValue = 0;
        private JoystickOffset? joyStickButton = null;
        public event PropertyChangedEventHandler? PropertyChanged;

        public JoystickOffset? JoyStickButton
        {
            get => joyStickButton; set
            {
                joyStickButton = value;
                OnPropertyChanged(nameof(JoyStickButtonString));
                OnPropertyChanged(nameof(IsJoyStickOffsetNotNull));
            }
        }

        public bool IsJoyStickOffsetNotNull
        {
            get => JoyStickButton is not null;

        }

        public int MaxValue
        {
            get => maxValue; set
            {
                maxValue = value;
                OnPropertyChanged();
            }
        }

        private JoyStick JoyStick { get; set; } = new(Guid.Empty, 0);

        public string JoyStickButtonString
        {
            get
            {
                if (JoyStickButton is not null)
                    return Enum.GetName((JoystickOffset)JoyStickButton) ?? "-";
                else
                    return "-";
            }
        }

        public JoyStickButtonSelection()
        {
            this.DataContext = this;
            InitializeComponent();
            JoyStick.OnValueUpdate += OnJoyStickValueUpdate;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public void OnJoyStickValueUpdate(Object? sender, JoyStickUpdateEventArgs e)
        {
            if (JoyStickButton is not null)
            {
                MaxValue = e.currentValue >= MaxValue ? e.currentValue : MaxValue;
            }
            else
            {
                JoyStickButton = e.joyStickOffset;
                MaxValue = e.currentValue;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();

        }

        private void jsbsWindow_Closing(object sender, CancelEventArgs e)
        {
            JoyStick.Dispose();
            if (MaxValue < 1)
                e.Cancel = MessageBoxResult.Cancel == MessageBox.Show("Achtung, bei einem maximal Wert von 0 funktioniert der Button nicht!", "Error", MessageBoxButton.OKCancel);
            if (JoyStickButton is null)
                e.Cancel = MessageBoxResult.Cancel == MessageBox.Show("Achtung, es wurde kein Button ausgewählt!", "Error", MessageBoxButton.OKCancel);
            this.DialogResult = MaxValue >= 1 && JoyStickButton is not null;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            JoyStickButton = null;
            maxValue = 0;
        }
    }
}
