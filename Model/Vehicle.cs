using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

#nullable disable

namespace Model
{
    public partial class Vehicle : IEquatable<Vehicle>, IComparable
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string ImageName { get; set; } = "";

        public VehicleType Type { get; set; } = VehicleType.Lokomotive;

        public long? MaxSpeed { get; set; } = 0;

        public long Speed​​step { get; set; } = 128;

        [Required]
        public long Address { get; set; } = 3;

        public bool IsActive { get; set; } = true;

        public long Position { get; set; } = 0;

        public string DriversCab { get; set; } = "";

        public string FullName { get; set; } = "";

        public long? SpeedDisplay { get; set; } = 0;

        public string Railway { get; set; } = "";

        public long BufferLenght { get; set; }

        public long ModelBufferLenght { get; set; }

        public long ServiceWeight { get; set; }

        public long ModelWeight { get; set; }

        public long Rmin { get; set; }

        public string Manufacturer { get; set; } = "";

        public string ArticleNumber { get; set; } = "";

        public string DecoderType { get; set; } = "";

        public string Owner { get; set; } = "";

        public string BuildYear { get; set; } = "";

        public string OwningSince { get; set; } = "";

        public bool InvertTraction { get; set; }

        public string Description { get; set; } = "";

        public bool? Dummy { get; set; } = false;

        public IPAddress Ip { get; set; } = IPAddress.None;

        public long? Video { get; set; } = 0;

        public long? DirectSteering { get; set; } = 0;

        public bool? Crane { get; set; } = false;

        public Category Category { get; set; } = default!;

        public int CategoryId { get; set; }

        public Epoch Epoche { get; set; } = Epoch.@default;

        public List<Function> Functions { get; set; } = new();

        public decimal?[] TractionForward { get; set; } = new decimal?[127 + 1];

        public decimal?[] TractionBackward { get; set; } = new decimal?[127 + 1];

        public List<int> TractionVehicleIds { get; set; } = new();

        public int CompareTo(object obj) => Id.CompareTo(obj);

        public bool Equals(Vehicle other) => Id == other?.Id;

        /// <summary>
        /// Gets the real world speed for a given speed step and direction
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public decimal? GetTractionSpeed(int speed, bool direction) => (speed <= 127 && speed >= 2) ? (direction ? TractionForward[speed] : TractionBackward[speed]) : throw new ArgumentOutOfRangeException();

        public override string ToString() => $"Add: {Address} - Name: \"{Name ?? FullName}\" - Pos: {Position} - {(IsActive ? "Aktiv" : "Deaktiviert")}";
    }
}
