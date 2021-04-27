﻿using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

namespace WPF_Application.JoyStick
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
            }
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
            if (JoyStickButton == e.joyStickOffset)
            {
                MaxValue = e.currentValue >= MaxValue ? e.currentValue : MaxValue;
            }
            else
            {
                MaxValue = e.currentValue;
            }
            JoyStickButton = e.joyStickOffset;
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
    }
}