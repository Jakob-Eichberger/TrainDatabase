using Helper;
using System;
using System.Runtime.Serialization;

namespace WPF_Application.Exceptions
{
    public class ControlerException : Exception
    {
        public ControlerException(ModelTrainController.ModelTrainController controler)
        {
            controler.SetTrackPowerOFF();
        }

        public ControlerException(ModelTrainController.ModelTrainController controler, string message) : base(message)
        {
            controler.SetTrackPowerOFF();
        }

        public ControlerException(ModelTrainController.ModelTrainController controler, string message, Exception innerException) : base(message, innerException)
        {
            controler.SetTrackPowerOFF();
        }

        protected ControlerException(ModelTrainController.ModelTrainController controler, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            controler.SetTrackPowerOFF();
        }
    }
}
