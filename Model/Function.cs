#nullable disable

namespace Model
{
    public partial class Function
    {
        public int Id { get; set; }

        public Vehicle Vehicle { get; set; }

        public int VehicleId { get; set; }

        public ButtonType ButtonType { get; set; }

        public bool IsActive { get; set; } = true;

        public string Name { get; set; }

        public decimal Time { get; set; }

        public int Position { get; set; }

        public string ImageName { get; set; }

        public int FunctionIndex { get; set; }

        public bool ShowFunctionNumber { get; set; }

        public bool IsConfigured { get; set; }

        public FunctionType EnumType { get; set; }

        public override bool Equals(object obj) => obj is Function function && Id == function?.Id;

        public override string ToString() =>
            $"Name: \t{Name}\n" +
            $"Adresse:\t{FunctionIndex}";
    }
}
