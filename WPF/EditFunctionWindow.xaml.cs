using Model;
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
using WPF_Application.Infrastructure;
using WPF_Application.Services;

namespace WPF_Application
{
    /// <summary>
    /// Interaction logic for EditFunctionWindow.xaml
    /// </summary>
    public partial class EditFunctionWindow : Window, INotifyPropertyChanged
    {
        private Database db;
        public event PropertyChangedEventHandler? PropertyChanged;
        private FunctionService functionService;
        private Function function = default!;

        public Function Function
        {
            get => function; set
            {
                function = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<FunctionType> EnumList
        {
            get => Enum.GetValues(typeof(FunctionType)).Cast<FunctionType>().Where(e => (int)e > 3 || (int)e == 0);
        }

        public EditFunctionWindow(Database _db, Function function)
        {
            this.DataContext = this;
            InitializeComponent();
            if (_db is null) throw new ApplicationException($"Paramter '{nameof(_db)}' darf nicht null sein!");
            if (function is null) throw new ApplicationException($"Paramter '{nameof(function)}' darf nicht null sein!");
            db = _db;
            Function = db.Functions.FirstOrDefault(e => e.Id == function.Id)!;
            if (Function is null) throw new ApplicationException($"Funktion  mit der ID '{function.Id} konnte nicht geöffnet werden!");
            functionService = new(db);
            this.Name = Function.Name;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null!) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            functionService.Update(Function);
            this.DialogResult = true;
            this.Close();
        }
    }
}
