using System;
using WPF_Application.CentralStation.DTO;

namespace WPF_Application.CentralStation.Events
{
    public class GetLocoFunctionInfoEventArgs : EventArgs
    {
        public GetLocoFunctionInfoEventArgs(LokInfoFunctionData data) : base()
        {
            Data = data;
        }
        public LokInfoFunctionData Data;
    }
}