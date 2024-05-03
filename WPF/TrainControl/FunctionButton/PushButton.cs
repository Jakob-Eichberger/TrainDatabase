using Model;
using System;

namespace WPF_Application.TrainControl.FunctionButton
{
    internal class PushButton : FunctionBase
    {
        public PushButton(IServiceProvider serviceProvider, FunctionModel functionModel) : base(serviceProvider, functionModel)
        {
            PreviewMouseDown += (sender, e) => Function.SetState(true);
            PreviewMouseUp += (sender, e) => Function.SetState(false);
        }
    }
}
