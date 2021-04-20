using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helper
{
    public static class Engine
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentSpeed">This speed should bin in Steps (between 0 - 126)</param>
        /// <param name="totalMass">Total Mass in KG</param>
        /// <param name="newton">The force in newton.</param>
        /// <param name="maxSpeed">The max speed of the Loko in KM/H</param>
        /// <returns></returns>
        public static int GetNextSpeedStep(int currentSpeed, decimal totalMass, int maxSpeed, decimal newton, int maxSpeedStep = 126)
        {
            //a = f/m

            //a = acceleration in m/s^2
            //f = Netwon
            //m = Mas in kg

            //a -> Beschleunigung in km/h
            var a = (newton / totalMass) * (decimal)3.6;
            var currentkmh = ((decimal)maxSpeed * (decimal)currentSpeed) / (decimal)maxSpeedStep;
            var c = currentkmh + a;

            return (int)(((c * maxSpeedStep) / maxSpeed)); ;
        }

    }
}
