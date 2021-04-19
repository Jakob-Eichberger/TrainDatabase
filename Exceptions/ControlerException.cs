using Helper;
using System;
using System.Runtime.Serialization;

namespace Exceptions
{
    public class ControlerException : Exception
    {
        public ControlerException(Z21 z21)
        {
            z21.Nothalt();
        }

        public ControlerException(Z21 z21, string message) : base(message)
        {
            z21.Nothalt();
        }

        public ControlerException(Z21 z21, string message, Exception innerException) : base(message, innerException)
        {
            z21.Nothalt();
        }

        protected ControlerException(Z21 z21, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            z21.Nothalt();
        }
    }
}
