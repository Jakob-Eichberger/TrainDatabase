using Model;
using System;
using System.Threading.Tasks;

namespace WPF_Application.TrainControl.FunctionButton
{
  internal class TimerButton : FunctionBase
  {
    public TimerButton(IServiceProvider serviceProvider, FunctionModel functionModel) : base(
                                                                                             serviceProvider,
                                                                                             functionModel)
    {
      PreviewMouseDown += async (sender, e) =>
                          {
                            Function.SetState(true);
                            await Task.Delay(new TimeSpan(0, 0, functionModel.Time));
                            Function.SetState(false);
                          };
      Function.StateChanged += (a, state) => Dispatcher.Invoke(() => IsEnabled = !state);
    }
  }
}