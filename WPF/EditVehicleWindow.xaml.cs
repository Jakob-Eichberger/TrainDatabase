using Microsoft.EntityFrameworkCore;
using Model;
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
using System.Windows.Shapes;
using WPF_Application.Infrastructure;

namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for EditVehicleWindow.xaml
    /// </summary>
    public partial class EditVehicleWindow : Window
    {
        public Vehicle Vehicle { get; set; }
        public readonly Database db;
        public EditVehicleWindow(Database _db, Vehicle _vehicle)
        {
            InitializeComponent();
            if (_vehicle is null) throw new ApplicationException($"Paramter '{nameof(_vehicle)}' darf nicht null sein!");
            if (_db is null) throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");
            db = _db;
            Vehicle = db.Vehicles.Include(i => i.Functions).FirstOrDefault(e => e.Id == _vehicle.Id) ?? throw new ApplicationException($"Fahrzeg mit der Id'{_vehicle.Id}' wurde nicht in der Datenbank gefunden!");
            DrawAllFunctions();
        }


        private void DrawAllFunctions()
        {
            SPFunctions.Children.Clear();
            Grid functionGrid = new();
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            functionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            foreach (var item in Vehicle.Functions)
            {
                Label enumLabel = new() { Content = item.ImageName, Margin = new Thickness(0, 0, 20, 5) };
                enumLabel.MouseDoubleClick += EditFunction_Click;
                Grid.SetColumn(enumLabel, 0);
                Grid.SetRow(enumLabel, functionGrid.RowDefinitions.Count);
                SPFunctions.Children.Add(enumLabel);
            }
        }
        private void EditFunction_Click(object? sender, EventArgs e)
        {

        }
    }
}
