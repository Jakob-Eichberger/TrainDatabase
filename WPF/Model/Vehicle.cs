using ModelTrainController;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

#nullable disable

namespace Model
{
    public partial class Vehicle : IEquatable<Vehicle>, IComparable
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string Image_Name { get; set; } = "";

        public VehicleType Type { get; set; } = VehicleType.Lokomotive;

        public long? Max_Speed { get; set; } = 0;

        public long Speed​​step { get; set; } = 128;

        [Required]
        public long Address { get; set; } = 0;

        public bool Active { get; set; } = true;

        public long Position { get; set; } = 0;

        public string Drivers_Cab { get; set; } = "";

        public string Full_Name { get; set; } = "";

        public long? Speed_Display { get; set; } = 0;

        public string Railway { get; set; } = "";

        public long Buffer_Lenght { get; set; }

        public long Model_Buffer_Lenght { get; set; }

        public long Service_Weight { get; set; }

        public long Model_Weight { get; set; }

        public long Rmin { get; set; }

        public string Article_Number { get; set; } = "";

        public string Decoder_Type { get; set; } = "";

        public string Owner { get; set; } = "";

        public string Build_Year { get; set; } = "";

        public string Owning_Since { get; set; } = "";

        public bool InvertTraction { get; set; }

        public string Description { get; set; } = "";

        public bool? Dummy { get; set; } = false;

        public IPAddress Ip { get; set; } = IPAddress.None;

        public long? Video { get; set; } = 0;

        public long? Direct_Steering { get; set; } = 0;

        public bool? Crane { get; set; } = false;

        public Category Category { get; set; } = default!;

        public int CategoryId { get; set; }

        public Epoch Epoche { get; set; } = Epoch.@default;

        public List<Function> Functions { get; set; } = new();

        public decimal?[] TractionForward { get; set; } = new decimal?[CentralStationClient.maxDccStep + 1];

        public decimal?[] TractionBackward { get; set; } = new decimal?[CentralStationClient.maxDccStep + 1];

        public int CompareTo(object obj) => Id.CompareTo(obj);

        public bool Equals(Vehicle other) => Id == other?.Id;

        /// <summary>
        /// Gets the real world speed for a given speed step and direction
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public decimal? GetTractionSpeed(int speed, bool direction) => (speed <= 127 && speed >= 2) ? (direction ? TractionForward[speed] : TractionBackward[speed]) : throw new ArgumentOutOfRangeException();

    }
}
