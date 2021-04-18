using Helper;
using System;
using System.Runtime.Serialization;

namespace Exceptions
{
    public class Z21Exception : Exception
    {
        public Z21Exception(Z21 z21)
        {
            z21.Nothalt();
        }

        public Z21Exception(Z21 z21, string message) : base(message)
        {
            z21.Nothalt();
        }

        public Z21Exception(Z21 z21, string message, Exception innerException) : base(message, innerException)
        {
            z21.Nothalt();
        }

        protected Z21Exception(Z21 z21, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            z21.Nothalt();
        }
    }
}
