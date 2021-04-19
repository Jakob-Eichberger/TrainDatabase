using Helper;
using System;
using System.Runtime.Serialization;

namespace Exceptions
{
    public class ControlerException : Exception
    {
        public ControlerException(ModelTrainController.ModelTrainController controler)
        {
            controler.Nothalt();
        }

        public ControlerException(ModelTrainController.ModelTrainController controler, string message) : base(message)
        {
            controler.Nothalt();
        }

        public ControlerException(ModelTrainController.ModelTrainController controler, string message, Exception innerException) : base(message, innerException)
        {
            controler.Nothalt();
        }

        protected ControlerException(ModelTrainController.ModelTrainController controler, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            controler.Nothalt();
        }
    }
}
