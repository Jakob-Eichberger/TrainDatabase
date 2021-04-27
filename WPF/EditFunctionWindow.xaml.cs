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
using WPF_Application.Services;

namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for EditFunctionWindow.xaml
    /// </summary>
    public partial class EditFunctionWindow : Window
    {
        private Database db;
        private FunctionService functionService;
        private Function Function { get; set; } = default!;

        public EditFunctionWindow(Database _db, Function function)
        {
            InitializeComponent();
            if (_db is null) throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");
            if (function is null) throw new ApplicationException($"Paramter '{nameof(function)}' darf nicht null sein!");
            if (Function is null) throw new ApplicationException($"Funktion  mit der ID '{function.Id} konnte nicht geöffnet werden!");
            db = _db;
            Function = db.Functions.FirstOrDefault(e => e.Id == function.Id)!;
            functionService = new(db);
            this.Name = Function.Shortcut;
        }
    }
}
