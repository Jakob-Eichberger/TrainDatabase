using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Model;
using Wpf_Application;

#nullable disable

namespace Infrastructure
{
    public partial class Database : DbContext
    {
        public Database()
        {
        }

        public Database(DbContextOptions<Database> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<DcFunction> DcFunctions { get; set; }
        public virtual DbSet<Function> Functions { get; set; }
        public virtual DbSet<LayoutDatum> LayoutData { get; set; }
        public virtual DbSet<TractionList> TractionLists { get; set; }
        public virtual DbSet<TrainList> TrainLists { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=Loco.sqlite");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>().HasMany(e => e.Functions);
            modelBuilder.Entity<Vehicle>().HasOne(e => e.Category);
            modelBuilder.Entity<Category>().HasMany(e => e.Vehicles);
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);


        public void FillDatabase()
        {
            for (int i = 0; i < 20; i++)
            {
                Vehicles.Add(new Vehicle
                {
                    Active = true,
                    Address = i,
                    Name = $"Name {i}",
                    ImageName = "57AE7DD0-ED68-462C-A5C2-B9B29710CDA1.png",
                    Position = i
                });
            }
            SaveChanges();
        }
    }
}
