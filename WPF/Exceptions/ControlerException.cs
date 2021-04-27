using Helper;
using System;
using System.Runtime.Serialization;

namespace WPF_Application.Exceptions
{
    public class ControlerException : Exception
    {
        public ControlerException(ModelTrainController.CentralStationClient controler)
        {
            controler.SetTrackPowerOFF();
        }

        public ControlerException(ModelTrainController.CentralStationClient controler, string message) : base(message)
        {
            controler.SetTrackPowerOFF();
        }

        public ControlerException(ModelTrainController.CentralStationClient controler, string message, Exception innerException) : base(message, innerException)
        {
            controler.SetTrackPowerOFF();
        }

        protected ControlerException(ModelTrainController.CentralStationClient controler, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            controler.SetTrackPowerOFF();
        }
    }
}
