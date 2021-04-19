using System;
using System.Net.Sockets;

namespace ModelTrainController
{
    public abstract class IModelTrainController : UdpClient
    {
        public IModelTrainController(StartData startData) : base(startData.LanPort)
        {

        }
    }
}
