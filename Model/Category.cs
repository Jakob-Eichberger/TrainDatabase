using System;
using System.Collections.Generic;

#nullable disable

namespace Model
{
    public partial class Category
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public List<Vehicle> Vehicles { get; set; }
    }
}
