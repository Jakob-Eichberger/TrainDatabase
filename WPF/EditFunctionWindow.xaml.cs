using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using WPF_Application.Infrastructure;

namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for EditFunctionWindow.xaml
    /// </summary>
    public partial class EditFunctionWindow : Window, INotifyPropertyChanged
    {
        private readonly Database db;

        public event PropertyChangedEventHandler? PropertyChanged;

        private Function? function = new();
        private Vehicle? Vehicle { get; set; } = null;

        public Function Function
        {
            get => function; set
            {
                function = value;
                OnPropertyChanged();
            }
        }

        public static IEnumerable<FunctionType> EnumList => Enum.GetValues(typeof(FunctionType)).Cast<FunctionType>().Where(e => (int)e > 3 || (int)e == 0);

        public EditFunctionWindow(Database _db, Function function)
        {
            this.DataContext = this;
            InitializeComponent();
            db = _db ?? throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");
            if (function is null) throw new ApplicationException($"Paramter '{nameof(function)}' darf nicht null sein!");
            Function = db.Functions.Include(m => m.Vehicle).ThenInclude(m => m.Functions).FirstOrDefault(e => e.Id == function.Id) ?? throw new ApplicationException($"Funktion  mit der ID '{function.Id} konnte nicht geöffnet werden!");
            this.Title = Function.Name;
            BtnSaveAndClose.Click += SaveChanges_Click;
            BtnSaveAndClose.Content = "Speicher und schließen";

            switch (Function.ButtonType)
            {
                case ButtonType.PushButton:
                    RbPushButton.IsChecked = true;
                    break;
                case ButtonType.Switch:
                    RbSwitch.IsChecked = true;
                    break;
                case ButtonType.Timer:
                    RbTimer.IsChecked = true;
                    break;
            }
        }

        public EditFunctionWindow(Database _db, Vehicle _vehicle)
        {
            this.DataContext = this;
            InitializeComponent();
            db = _db ?? throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");
            Vehicle = db.Vehicles.Include(m => m.Functions).FirstOrDefault(m => m.Id == _vehicle.Id) ?? throw new ApplicationException($"Vehicle with address '{_vehicle.Address}' was not found!");
            Function = new();
            this.Title = "Neue Funktion";
            BtnSaveAndClose.Click += AddFunction_Click;
            BtnSaveAndClose.Content = "Speicher und schließen";
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValid())
            {
                MessageBox.Show($"Der Funktionsname ist nicht valide!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            this.DialogResult = true;
            db.Update(Function);
            this.Close();
        }

        private void AddFunction_Click(object sender, RoutedEventArgs e)
        {
            if (Vehicle is not null)
            {
                if (!IsValid())
                {
                    MessageBox.Show($"Der Funktionsname ist nicht valide!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                this.DialogResult = true;
                Vehicle.Functions.Add(Function);
                db.Update(Vehicle);
                this.Close();
            }
        }

        public bool IsValid() => !string.IsNullOrWhiteSpace(Function?.Name ?? null);

        private void TypeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            Function.ButtonType = (ButtonType)Enum.Parse(typeof(ButtonType), (sender as RadioButton)!.Tag!.ToString()!);
        }
    }
}
