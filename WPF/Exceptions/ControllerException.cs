using System;
using System.Runtime.Serialization;

namespace WPF_Application.Exceptions
{
    public class ControllerException : Exception
    {
        public ControllerException(ModelTrainController.CentralStationClient controler)
        {
            controler.SetTrackPowerOFF();
        }

        public ControllerException(ModelTrainController.CentralStationClient controler, string message) : base(message)
        {
            controler.SetTrackPowerOFF();
        }

        public ControllerException(ModelTrainController.CentralStationClient controler, string message, Exception innerException) : base(message, innerException)
        {
            controler.SetTrackPowerOFF();
        }

        protected ControllerException(ModelTrainController.CentralStationClient controler, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            controler.SetTrackPowerOFF();
        }
    }
}
