using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WPF_Application.Helper;

namespace WPF_Application.JoyStick
{
    public class JoyStick
    {
        public Guid joystickGuid = Guid.Empty;
        public int rate;
        public Joystick? Joystick { get; set; }

        Dictionary<JoystickOffset, int> JoyStickMaxValue { get; set; } = new();

        /// <summary>
        /// Gets called whener a poll happens. 
        /// </summary>
        public event EventHandler<JoyStickUpdateEventArgs> OnValueUpdate = default!;

        public JoyStick(Guid joystickGuid, int _rate = 1)
        {
            this.joystickGuid = joystickGuid;
            rate = _rate;
            JoyStickMaxValue = Settings.GetJoyStickMaxValue();
            Guid guid = GetAllJoySticks().FirstOrDefault();
            if (guid != Guid.Empty)
            {
                Joystick = new Joystick(new DirectInput(), guid);
                Joystick.Properties.BufferSize = 128;
                Joystick.Acquire();
                Run();
            }
        }

        /// <summary>
        /// Disposes of a Joystick. Does not throw an exception.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (Joystick is not null)
                {
                    Joystick.Dispose();
                }
            }
            catch
            {
                //Does nothing.
            }
        }

        /// <summary>
        /// Polls the Joystck and creates Events for every single update.
        /// </summary>
        private void Run()
        {

            new Thread(() =>
            {
                try
                {
                    if (Joystick is null || Joystick.IsDisposed) return;
                    while (Joystick is not null && !Joystick.IsDisposed)
                    {
                        Joystick.Poll();
                        var datas = Joystick.GetBufferedData();
                        foreach (var state in datas.ToList())
                        {
                            if (OnValueUpdate is not null) OnValueUpdate(this, new JoyStickUpdateEventArgs(state.Offset, state.Value, JoyStickMaxValue.TryGetValue(state.Offset, out int maxval) ? maxval : 0));
                        }
                    }
                }
                catch (SharpDX.SharpDXException ex)
                {
                    Logger.Log($"Fehler by {nameof(Joystick.GetBufferedData)}", true, ex);
                }
            }).Start();

        }

        /// <summary>
        /// Gets a List of every Joystick plugged into Windows.
        /// </summary>
        /// <returns>A List of Joysticks represented as  <see cref="Guid"/></returns>
        public static List<Guid> GetAllJoySticks() => new List<Guid>(new DirectInput().GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices).ToList().Select(e => e.InstanceGuid)) ?? new();
    }
}
