using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;
using System.Text.RegularExpressions;

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

        private void Arduino_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ResetTimer();
            foreach (var item in Regex.Replace(ReadExisting().Trim(), @"\s+", string.Empty).ToCharArray())
                SerialBuffer.Enqueue(item);
            ResetTimer();
        }

        private void ResetTimer()
        {
            Timer.Stop();
            Timer.Start();
        }

        private void ResetTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs(String.Join("", SerialBuffer.ToList())));
            SerialBuffer.Clear();
        }

        public class DataReceivedEventArgs : EventArgs
        {
            public DataReceivedEventArgs(string value)
            {
                Value = value;
            }

            public string Value { get; set; } = "";
        }

        public new void Dispose() => Close();

        /// <summary>
        /// Resets the Arduino.
        /// </summary>
        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
