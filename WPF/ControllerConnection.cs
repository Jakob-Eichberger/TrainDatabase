using Helper;
using ModelTrainController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WPF_Application
{
    static class ControllerConnection
    {

        [Obsolete("Not yet implemented, returns default!", false)]
        /// <summary>
        /// Function gets the LanAdress and LanPort from a config file and creates a new <see cref="Z21"/> object.
        /// </summary>
        /// <returns>Retuns a <see cref="Z21"/> object.</returns>
        public static StartData Get() => new StartData { LanAdresse = "192.168.0.111", LanPort = 21105 };

        /// <summary>
        /// Function sets an <paramref name="address"/> and a <paramref name="port"/> which it then saves in a config file.
        /// </summary>
        /// <param name="address">IpAddress object.</param>
        /// <param name="port">Port</param>
        public static void Set(IPAddress address, int port)
        {
            throw new NotImplementedException("doesnt work yet");
        }
    }
}
