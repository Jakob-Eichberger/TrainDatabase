using System;
using System.Runtime.Serialization;
using TrainDatabase.Z21Client;

namespace TrainDatabase.Exceptions
{
    public class ControllerException : Exception
    {
        public ControllerException(Z21Client.Z21Client controller)
        {
            controller.SetTrackPowerOFF();
        }

        public ControllerException(Z21Client.Z21Client controller, string message) : base(message)
        {
            controller.SetTrackPowerOFF();
        }

        public ControllerException(Z21Client.Z21Client controller, string message, Exception innerException) : base(message, innerException)
        {
            controller.SetTrackPowerOFF();
        }

        protected ControllerException(Z21Client.Z21Client controller, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            controller.SetTrackPowerOFF();
        }
    }
}
