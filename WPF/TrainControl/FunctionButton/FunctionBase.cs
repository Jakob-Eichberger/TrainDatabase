using Model;
using Service.Controller;
using System;
using System.Windows.Controls;

namespace WPF_Application.TrainControl.FunctionButton
{
    internal abstract class FunctionBase : Button
    {
        public FunctionBase(IServiceProvider serviceProvider, FunctionModel functionModel)
        {
            ServiceProvider = serviceProvider;
            FunctionModel = functionModel;
            FunctionButton.ApplyStyle(this, FunctionModel);

            Function = new FunctionController(ServiceProvider, functionModel);
        }

        private IServiceProvider ServiceProvider { get; }

        private FunctionModel FunctionModel { get; }

        internal FunctionController Function { get; }
    }
}
