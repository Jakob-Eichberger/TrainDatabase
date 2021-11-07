using System;
using System.Runtime.Serialization;
using WPF_Application.CentralStation;

namespace WPF_Application.Exceptions
{
    public class ControllerException : Exception
    {
        public ControllerException(CentralStationClient controller)
        {
            controller.SetTrackPowerOFF();
        }

        public ControllerException(CentralStationClient controller, string message) : base(message)
        {
            controller.SetTrackPowerOFF();
        }

        public ControllerException(CentralStationClient controller, string message, Exception innerException) : base(message, innerException)
        {
            controller.SetTrackPowerOFF();
        }

        protected ControllerException(CentralStationClient controller, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            controller.SetTrackPowerOFF();
        }
    }
}
