using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Helper
{
    public class JoyStick
    {
        public Joystick? Joystick { get; set; }

        /// <summary>
        /// Gets called whener a poll happens. 
        /// </summary>
        public event EventHandler<JoyStickUpdateEventArgs> OnValueUpdate = default!;

        public Guid joystickGuid = Guid.Empty;
        public JoyStick(Guid joystickGuid)
        {
            this.joystickGuid = joystickGuid;
            //TODO : check that device actually exists.
        }


        /// <summary>
        /// Tries to aquire a Joystick. Does not throw an exception.
        /// </summary>
        public void Acquire()
        {
            try
            {
                if (Joystick is null || Joystick.IsDisposed)
                {
                    Guid guid = Helper.JoyStick.GetAllJoySticks().FirstOrDefault();
                    if (guid != Guid.Empty)
                    {
                        Joystick = new Joystick(new DirectInput(), guid);
                        Joystick.Properties.BufferSize = 128;
                        Joystick.Acquire();
                        Run();
                    }
                }
            }
            catch
            {
                //Do nothing.
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
                if (Joystick is null || Joystick.IsDisposed) return;
                while (Joystick is not null && !Joystick.IsDisposed)
                {
                    Joystick.Poll();
                    var datas = Joystick.GetBufferedData();
                    foreach (var state in datas.ToList().Where(e => e.Offset == JoystickOffset.Z))
                    {
                        if (OnValueUpdate is not null) OnValueUpdate(this, new JoyStickUpdateEventArgs(state.Offset, state.Value, 65535));
                    }
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
