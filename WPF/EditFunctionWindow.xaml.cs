﻿using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace TrainDatabase
{
    /// <summary>
    /// Interaction logic for EditFunctionWindow.xaml
    /// </summary>
    public partial class EditFunctionWindow : Window, INotifyPropertyChanged
    {
        private readonly Database db;

        public event PropertyChangedEventHandler? PropertyChanged;

        private FunctionModel function = new();

        public FunctionModel Function
        {
            get => function; set
            {
                function = value;
                OnPropertyChanged();
            }
        }

        public EditFunctionWindow(Database _db, FunctionModel function)
        {
            DataContext = this;
            InitializeComponent();
            db = _db ?? throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");

            if (function is null)
                throw new ApplicationException($"Paramter '{nameof(function)}' darf nicht null sein!");

            Function = db.Functions.Include(m => m.Vehicle).ThenInclude(m => m.Functions).FirstOrDefault(e => e.Id == function.Id) ?? throw new ApplicationException($"Funktion  mit der ID '{function.Id} konnte nicht geöffnet werden!");

            Title = Function.Name ?? "";

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

        protected void OnPropertyChanged([CallerMemberName] string name = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void TypeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            Function.ButtonType = (ButtonType)Enum.Parse(typeof(ButtonType), (sender as RadioButton)!.Tag!.ToString()!);
        }
    }
}
