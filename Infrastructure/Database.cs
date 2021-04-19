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

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<DcFunction>(entity =>
            {
                entity.ToTable("dc_functions");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.ButtonType)
                    .HasColumnType("INT")
                    .HasColumnName("button_type");

                entity.Property(e => e.CabFunctionDescription).HasColumnName("cab_function_description");

                entity.Property(e => e.DriversCab).HasColumnName("drivers_cab");

                entity.Property(e => e.Function).HasColumnName("function");

                entity.Property(e => e.ImageName).HasColumnName("image_name");

                entity.Property(e => e.IsConfigured).HasColumnName("is_configured");

                entity.Property(e => e.Position).HasColumnName("position");

                entity.Property(e => e.Shortcut)
                    .IsRequired()
                    .HasColumnName("shortcut")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ShowFunctionNumber)
                    .HasColumnName("show_function_number")
                    .HasDefaultValueSql("1");

                entity.Property(e => e.Time).HasColumnName("time");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            });

            modelBuilder.Entity<Function>(entity =>
            {
                entity.ToTable("functions");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.ButtonType).HasColumnName("button_type");

                entity.Property(e => e.Function1).HasColumnName("function");

                entity.Property(e => e.ImageName).HasColumnName("image_name");

                entity.Property(e => e.IsConfigured).HasColumnName("is_configured");

                entity.Property(e => e.Position).HasColumnName("position");

                entity.Property(e => e.Shortcut)
                    .IsRequired()
                    .HasColumnName("shortcut")
                    .HasDefaultValueSql("''");

                entity.Property(e => e.ShowFunctionNumber)
                    .HasColumnName("show_function_number")
                    .HasDefaultValueSql("1");

                entity.Property(e => e.Time).HasColumnName("time");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            });

            modelBuilder.Entity<LayoutDatum>(entity =>
            {
                entity.ToTable("layout_data");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.ControlStationTheme)
                    .HasColumnName("control_station_theme")
                    .HasDefaultValueSql("'free'");

                entity.Property(e => e.ControlStationType)
                    .HasColumnName("control_station_type")
                    .HasDefaultValueSql("'free'");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<TractionList>(entity =>
            {
                entity.ToTable("traction_list");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.LocoId).HasColumnName("loco_id");

                entity.Property(e => e.RegulationStep).HasColumnName("regulation_step");

                entity.Property(e => e.Time).HasColumnName("time");
            });

            modelBuilder.Entity<TrainList>(entity =>
            {
                entity.ToTable("train_list");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Position).HasColumnName("position");

                entity.Property(e => e.TrainId).HasColumnName("train_id");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("vehicles");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Active).HasColumnName("active");

                entity.Property(e => e.Address).HasColumnName("address");

                entity.Property(e => e.ArticleNumber).HasColumnName("article_number");

                entity.Property(e => e.BufferLenght).HasColumnName("buffer_lenght");

                entity.Property(e => e.BuildYear).HasColumnName("build_year");

                entity.Property(e => e.Crane)
                    .HasColumnName("crane")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.DecoderType).HasColumnName("decoder_type");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.DirectSteering).HasColumnName("direct_steering");

                entity.Property(e => e.DriversCab).HasColumnName("drivers_cab");

                entity.Property(e => e.Dummy).HasColumnName("dummy");

                entity.Property(e => e.FullName).HasColumnName("full_name");

                entity.Property(e => e.ImageName).HasColumnName("image_name");

                entity.Property(e => e.Ip).HasColumnName("ip");

                entity.Property(e => e.MaxSpeed).HasColumnName("max_speed");

                entity.Property(e => e.ModelBufferLenght).HasColumnName("model_buffer_lenght");

                entity.Property(e => e.ModelWeight).HasColumnName("model_weight");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.Owner).HasColumnName("owner");

                entity.Property(e => e.OwningSince).HasColumnName("owning_since");

                entity.Property(e => e.Position).HasColumnName("position");

                entity.Property(e => e.Railway).HasColumnName("railway");

                entity.Property(e => e.Rmin).HasColumnName("rmin");

                entity.Property(e => e.ServiceWeight).HasColumnName("service_weight");

                entity.Property(e => e.SpeedDisplay).HasColumnName("speed_display");

                entity.Property(e => e.TractionDirection).HasColumnName("traction_direction");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.Video).HasColumnName("video");

            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
