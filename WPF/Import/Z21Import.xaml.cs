using Extensions;
using Helper;
using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Model;
using Service;
using Service.ImportService.Z21;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Importer
{
  /// <summary>
  /// Interaction logic for Z21.xaml
  /// </summary>
  public partial class Z21Import : Window, INotifyPropertyChanged
  {
    private readonly Database db;

    public Z21Import(IServiceProvider provider)
    {
      DataContext = this;
      InitializeComponent();
      db = provider.GetService<Database>()!;
      LogService = provider.GetService<LogEventBus>()!;
    }

    public LogEventBus LogService { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Path { get; set; } = "";

    protected void OnPropertyChanged([CallerMemberName] string name = null!)
    {
      PropertyChanged?.Invoke(this, new(name));
    }

    private async void BtnGo_Click(object sender, RoutedEventArgs e)
    {
      Z21ImportService z21 = new(db);
      await z21.ImportAsync(new(Path));
      MessageBox.Show($"Import Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
      Close();
    }

    private void BtnOpenFileDalog_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog ofp = new();
      ofp.DefaultExt = ".z21";
      ofp.Filter = "Z21 DB FIle (*.z21)|*.z21";
      ofp.ShowDialog();
      BtnImportNow.IsEnabled = !string.IsNullOrWhiteSpace(ofp.FileName);
      TbFileSelector.Text = ofp.FileName;
      Path = ofp.FileName;
    }

    //private FunctionType GetFunctionType(string name)
    //{
    //    Dictionary<string, FunctionType> dic = new();
    //    dic.Add("sound", FunctionType.Sound1);
    //    dic.Add("light", FunctionType.Light1);
    //    dic.Add("main_beam", FunctionType.MainBeam);
    //    dic.Add("main_beam2", FunctionType.LowBeam);

    //    if (dic.TryGetValue(name.ToLower(), out FunctionType func))
    //        return func;
    //    else if (Enum.TryParse<FunctionType>(name.Replace("_", ""), true, out var result))
    //        return result;
    //    else
    //        return FunctionType.None;
    //}
  }
}