using Helper;
using Microsoft.EntityFrameworkCore;
using Model;
using ModelTrainController;
using ModelTrainController.Z21;
using OxyPlot;
using OxyPlot.Series;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WPF_Application.Extensions;
using WPF_Application.Helper;
using WPF_Application.Infrastructure;
using WPF_Application.JoyStick;

namespace WPF_Application
{
    /// <summary>
    /// Partial class that holds every property and global variable for the <see cref="TrainControl"/> class.
    /// </summary>
    public partial class TrainControl
    {
        public Database db = default!;
        private Vehicle vehicle = default!;
        public ModelTrainController.CentralStationClient controller = default!;
        private LokInfoData liveData = new();
        private TrackPower trackPower;
        private bool lastTrackPowerUpdateWasShort = false;
        readonly Dictionary<FunctionType, (JoystickOffset joyStick, int maxValue)> functionToJoyStickDictionary = new();
        private int speed = 0;
        public event PropertyChangedEventHandler? PropertyChanged;
        public DateTime lastSpeedchange = DateTime.MinValue;

        /// <summary>
        /// Data directly from the Z21. Not Used to controll the vehicle. 
        /// </summary>
        public LokInfoData LiveData
        {
            get => liveData; set
            {
                liveData = value;
                if (!SliderInUser && (DateTime.Now - SliderLastused).TotalSeconds > 2)
                    Speed = value.Speed;
                OnPropertyChanged();
            }
        }

        public LokAdresse Adresse { get; set; } = default!;

        public bool InUse { get; set; } = default!;

        public int Speed
        {
            get => speed; set
            {
                if (value < 0 || value > CentralStationClient.maxDccStep) return;
                speed = value == 1 ? (speed > 1 ? 0 : 2) : value;
                if (LiveData.Speed != speed)
                    SetLocoDrive(speedstep: speed);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// True if the speed controll slider is currently used by the user. 
        /// </summary>
        public bool SliderInUser { get; set; }

        /// <summary>
        /// The date and time the user last used the speed controll slider.
        /// </summary>
        public DateTime SliderLastused { get; set; }

        /// <summary>
        /// The <see cref="Vehicle"/> the application is trying to controll
        /// </summary>
        public Vehicle Vehicle
        {
            get => vehicle;
            set => vehicle = value;
        }

        /// <summary>
        /// Data from the Z21.
        /// </summary>
        public TrackPower TrackPower
        {
            get => trackPower; set
            {
                if (!lastTrackPowerUpdateWasShort)
                {
                    trackPower = value;
                }
                lastTrackPowerUpdateWasShort = value == TrackPower.Short;
                OnPropertyChanged();
            }
        }

        public List<(Vehicle Vehicle, (SortedSet<FunctionPoint>? Forwards, SortedSet<FunctionPoint>? Backwards) Traction)> DoubleTractionVehicles { get; } = new();

        public static int MaxDccSpeed => CentralStationClient.maxDccStep;

        public GridLength VehicleTypeGridLength => (Vehicle?.Type ?? VehicleType.Lokomotive) == VehicleType.Lokomotive ? new GridLength(80) : new GridLength(0);

        public Visibility VehicleTypeVisbility => (Vehicle?.Type ?? VehicleType.Lokomotive) == VehicleType.Lokomotive ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// True if the TrackPower is on. False otherwhise. Used to set the trackpower.
        /// </summary>
        public bool TrackPowerBoolean
        {
            set
            {
                if (value)
                    controller.SetTrackPowerON();
                else
                    controller.SetTrackPowerOFF();
                OnPropertyChanged();
            }
            get => TrackPower.ToBoolean();
        }

        /// <summary>
        /// Gets a string describing the current track power mode. (Needed for UI)
        /// </summary>
        public string TrackPowerMessage => Enum.GetName(TrackPower)!;

        /// <summary>
        /// Returns a string that describes the current direction of travel. 
        /// </summary>
        public string GetDirectionString { get => LiveData.DrivingDirection ? "Vorwärts" : "Rückwärts"; }

        /// <summary>
        /// List of all buttons on a grid which controll vehicle <see cref="Function"/>s.
        /// </summary>
        private List<Button> FunctionButtons { get; set; } = new();

        /// <summary>
        /// List of all togglebuttons on a grid which controll vehicle <see cref="Function"/>s.
        /// </summary>
        private List<ToggleButton> FunctionToggleButtons { get; set; } = new();

        /// <summary>
        /// Holds the <see cref="Vehicle.Id"/> of the slowest Vehicle in the traktion list.
        /// </summary>
        public Vehicle SlowestVehicleInTractionList { get; set; }

        public new bool IsActive { get; set; } = false;

        private JoyStick.JoyStick? Joystick { get; }


    }

}
