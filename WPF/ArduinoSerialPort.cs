using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TrainDatabase
{
    /// <summary>
    /// Used to read the arduiono serial bus.
    /// </summary>
    public sealed class ArduinoSerialPort : SerialPort, IDisposable
    {
        public ArduinoSerialPort(string portName, int baudRate = 9600) : base(portName, baudRate)
        {
            Timer.Elapsed += ResetTimer_Elapsed;
            base.DataReceived += Arduino_DataReceived;
            DtrEnable = true;
            Open();
        }

        /// <summary>
        /// When a message is fully received the event is fired.
        /// </summary>
        public new event EventHandler<DataReceivedEventArgs>? DataReceived = null;


        private Semaphore Semaphore { get; set; } = new(0, 1);

        private Queue<char> SerialBuffer { get; } = new();

        private System.Timers.Timer Timer { get; set; } = new(200) { AutoReset = false, Enabled = false };

        /// <summary>
        /// Returns true if the supplied port<paramref name="name"/> has a device connected to it.
        /// </summary>
        /// <param name="name">The name of the port.</param>
        /// <returns></returns>
        public static bool PortAvailable(string name)
        {
            if (!GetPortNames().ToList().Any(e => e == name))
                return false;
            else
            {
                try
                {
                    new SerialPort(name).Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Disposes this object and closes the serial port.
        /// </summary>
        public new void Dispose() => Close();

        /// <summary>
        /// Resets the Arduino.
        /// </summary>
        public void Reset()
        {
            //https://www.theengineeringprojects.com/2015/11/reset-arduino-programmatically.html
            throw new NotImplementedException();
        }

        /// <summary>
        /// Blocks the current thread until some data has been read from the serial bus. 
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or System.Threading.Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <returns>Returns the read data.</returns>
        public async Task<string> WaitForValue(int millisecondsTimeout = -1) => await Task.Run(() =>
        {
            try
            {
                Semaphore.WaitOne(millisecondsTimeout);
                return String.Join("", SerialBuffer.ToList());
            }
            finally
            {
                SerialBuffer.Clear();
            }
        });

        private void Arduino_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ResetTimer();
            foreach (var item in Regex.Replace(ReadExisting().Trim(), @"\s+", string.Empty).ToCharArray())
                SerialBuffer.Enqueue(item);
            ResetTimer();
        }

        private void ReleaseSemaphore()
        {
            try
            {
                var f = Semaphore.Release();
            }
            catch (SemaphoreFullException) { }
        }

        private void ResetTimer()
        {
            if (!Timer.Enabled)
                SerialBuffer.Clear();
            Timer.Stop();
            Timer.Start();
        }

        private void ResetTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs(String.Join("", SerialBuffer.ToList())));
            ReleaseSemaphore();
        }

        public class DataReceivedEventArgs : EventArgs
        {
            public DataReceivedEventArgs(string value)
            {
                Value = value;
            }

            public string Value { get; set; } = "";
        }
    }

}
