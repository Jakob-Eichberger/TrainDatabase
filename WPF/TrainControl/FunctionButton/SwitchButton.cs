using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using Viewmodel;

namespace WPF_Application.TrainControl.FunctionButton
{
    internal class SwitchButton : ToggleButton
    {
        public SwitchButton(IServiceProvider serviceProvider, FunctionModel functionModel)
        {
            if (functionModel.ButtonType is not ButtonType.Switch)
                throw new ApplicationException($"Button is type {functionModel.ButtonType} but should be {ButtonType.Switch}");

            ServiceProvider = serviceProvider;
            FunctionModel = functionModel;
            FunctionButton.ApplyStyle(this, FunctionModel);

            Function = new Function(ServiceProvider, functionModel);
            Function.StateChanged += (a, state) => Dispatcher.Invoke(() => IsChecked = state);
            Click += (a, b) => Function.SetState(IsChecked ?? false);
        }

        private IServiceProvider ServiceProvider { get; }

        private FunctionModel FunctionModel { get; }

        private Function Function { get; }
    }
}
