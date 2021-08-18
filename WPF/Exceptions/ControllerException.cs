using System;
using System.Runtime.Serialization;

namespace WPF_Application.Exceptions
{
    public class ControllerException : Exception
    {
        public ControllerException(ModelTrainController.CentralStationClient controller)
        {
            controller.SetTrackPowerOFF();
        }

        public ControllerException(ModelTrainController.CentralStationClient controller, string message) : base(message)
        {
            controller.SetTrackPowerOFF();
        }

        public ControllerException(ModelTrainController.CentralStationClient controller, string message, Exception innerException) : base(message, innerException)
        {
            controller.SetTrackPowerOFF();
        }

        protected ControllerException(ModelTrainController.CentralStationClient controller, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            controller.SetTrackPowerOFF();
        }
    }
}
