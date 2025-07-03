using System;
using System.Runtime.Serialization;
using Z21;

namespace TrainDatabase.Exceptions
{
  public class ControllerException : Exception
  {
    public ControllerException(Client controller)
    {
      controller.SetTrackPowerOFF();
    }

    public ControllerException(Client controller, string message) : base(message)
    {
      controller.SetTrackPowerOFF();
    }

    public ControllerException(Client controller, string message, Exception innerException) : base(
                                                                                                   message,
                                                                                                   innerException)
    {
      controller.SetTrackPowerOFF();
    }

    protected ControllerException(Client controller, SerializationInfo info, StreamingContext context) :
      base(info, context)
    {
      controller.SetTrackPowerOFF();
    }
  }
}