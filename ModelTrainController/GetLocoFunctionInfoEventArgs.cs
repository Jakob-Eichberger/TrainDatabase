using System;

namespace ModelTrainController
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