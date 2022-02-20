using Extensions;
using Infrastructure;
using Model;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TrainDatabase.Z21Client.DTO;
using TrainDatabase.Z21Client.Enum;

namespace TrainDatabase
{
    /// <summary>
    /// Partial class that holds every property and global variable for the <see cref="TrainControl"/> class.
    /// </summary>
    public partial class TrainControl
    {
        public Z21Client.Z21Client controller = default!;
        public Database db = default!;
        public DateTime lastSpeedchange = DateTime.MinValue;
        private bool lastTrackPowerUpdateWasShort = false;
        private LokInfoData liveData = new();
        private int speed = 0;
        private TrackPower trackPower;
        private Vehicle vehicle = default!;
        public event PropertyChangedEventHandler? PropertyChanged;
        public static int MaxDccSpeed => Z21Client.Z21Client.maxDccStep;

        public LokAdresse Adresse { get; set; } = default!;

        public bool InUse { get; set; } = default!;

        public new bool IsActive { get; set; } = false;

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

        public List<MultiTractionItem> MultiTractionList { get; } = new();

        /// <summary>
        /// True if the speed controll slider is currently used by the user. 
        /// </summary>
        public bool SliderInUser { get; set; }

        /// <summary>
        /// The date and time the user last used the speed controll slider.
        /// </summary>
        public DateTime SliderLastused { get; set; }

        /// <summary>
        /// Holds the <see cref="Vehicle.Id"/> of the slowest Vehicle in the traktion list.
        /// </summary>
        public Vehicle SlowestVehicleInTractionList { get; set; }

        public int Speed
        {
            get => speed; set
            {
                if (value < 0 || value > Z21Client.Z21Client.maxDccStep)
                    return;

                speed = value == 1 ? (speed > 1 ? 0 : 2) : value;

                if (LiveData.Speed != speed)
                    SetLocoDrive(speedstep: speed);

                OnPropertyChanged();
            }
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
        /// The <see cref="Vehicle"/> the application is trying to controll
        /// </summary>
        public Vehicle Vehicle
        {
            get => vehicle;
            set => vehicle = value;
        }

        public GridLength VehicleTypeGridLength => (Vehicle?.Type ?? VehicleType.Lokomotive) == VehicleType.Lokomotive ? new GridLength(80) : new GridLength(0);

        public Visibility VehicleTypeVisbility => (Vehicle?.Type ?? VehicleType.Lokomotive) == VehicleType.Lokomotive ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// List of all buttons on a grid which controll vehicle <see cref="Function"/>s.
        /// </summary>
        private List<Button> FunctionButtons { get; set; } = new();

        /// <summary>
        /// List of all togglebuttons on a grid which controll vehicle <see cref="Function"/>s.
        /// </summary>
        private List<ToggleButton> FunctionToggleButtons { get; set; } = new();
    }
}