using Model;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WPF_Application.TrainControl.FunctionButton
{
    internal static class FunctionButton
    {
        internal static void ApplyStyle<T>(T button, FunctionModel functionModel) where T : ButtonBase
        {
            button.Height = 50;
            button.Width = 90;
            button.Content = $"{(functionModel.ShowFunctionNumber ? $"({functionModel.FunctionIndex}) " : "")}{functionModel.Name}";
            button.Tag = functionModel;
            button.ToolTip = $"{functionModel}";
            button.Margin = new(10);
            button.Padding = new(2);
            button.BorderBrush = Brushes.Black;
            button.BorderThickness = new(1);
        }
    }
}
