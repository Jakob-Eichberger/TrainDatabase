using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Z21;

#nullable disable

namespace Model
{
    public partial class VehicleModel : IEquatable<VehicleModel>, IComparable
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

        public string FullName { get; set; } = "";

        public string Railway { get; set; } = "";

        public bool InvertTraction { get; set; }

        public string Description { get; set; } = "";

        public bool? Dummy { get; set; } = false;

        public List<FunctionModel> Functions { get; set; } = new();

        public decimal?[] TractionForward { get; set; } = new decimal?[Client.maxDccStep + 1];

        public decimal?[] TractionBackward { get; set; } = new decimal?[Client.maxDccStep + 1];

        public List<int> TractionVehicleIds { get; } = new();

        public int CompareTo(object obj) => Id.CompareTo(obj);

        public bool Equals(VehicleModel other) => Id == other?.Id;

        /// <summary>
        /// Gets the real world speed for a given speed step and direction
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public decimal? GetTractionSpeed(int speed, bool direction) => (speed <= Client.maxDccStep && speed >= 2) ? (direction ? TractionForward[speed] : TractionBackward[speed]) : throw new ArgumentOutOfRangeException();

        public override string ToString() => $"Add: {Address} - Name: \"{Name ?? FullName}\" - Pos: {Position} - {(IsActive ? "Aktiv" : "Deaktiviert")}";
    }
}
