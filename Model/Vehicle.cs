using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

#nullable disable

namespace Model
{
    public partial class Vehicle
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string ImageName { get; set; } = "";

        public long? Type { get; set; } = 0;

        public long? MaxSpeed { get; set; } = 0;

        [Required]
        public long Address { get; set; } = 0;

        public bool Active { get; set; } = true;

        public long? Position { get; set; } = 0;

        public string DriversCab { get; set; } = "";

        public string FullName { get; set; } = "";

        public long? SpeedDisplay { get; set; } = 0;

        public string Railway { get; set; } = "";

        public string BufferLenght { get; set; } = "";

        public string ModelBufferLenght { get; set; } = "";

        public string ServiceWeight { get; set; } = "";

        public string ModelWeight { get; set; } = "";

        public string Rmin { get; set; } = "";

        public string ArticleNumber { get; set; } = "";

        public string DecoderType { get; set; } = "";

        public string Owner { get; set; } = "";

        public string BuildYear { get; set; } = "";

        public string OwningSince { get; set; } = "";

        public long? TractionDirection { get; set; } = 0;

        public string Description { get; set; } = "";

        public bool? Dummy { get; set; } = false;

        public IPAddress Ip { get; set; } = IPAddress.None;

        public long? Video { get; set; } = 0;

        public long? DirectSteering { get; set; } = 0;

        public bool? Crane { get; set; } = false;

        public Category Category { get; private set; } = default!;

        public int CategoryId { get; set; }

        public List<Function> Functions { get; private set; } = new();

    }
}
