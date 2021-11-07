using System;
using TrainDatabase.Z21Client.DTO;

namespace TrainDatabase.Z21Client.Events
{
    public class GetLocoFunctionInfoEventArgs : EventArgs
    {
        public LokInfoFunctionData Data;

        public GetLocoFunctionInfoEventArgs(LokInfoFunctionData data) : base()
        {
            Data = data;
        }
    }
}