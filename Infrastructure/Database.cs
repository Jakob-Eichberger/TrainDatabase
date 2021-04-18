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

        public virtual DbSet<AndroidMetadatum> AndroidMetadata { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<ControlStationControl> ControlStationControls { get; set; }
        public virtual DbSet<ControlStationControlState> ControlStationControlStates { get; set; }
        public virtual DbSet<ControlStationImage> ControlStationImages { get; set; }
        public virtual DbSet<ControlStationNote> ControlStationNotes { get; set; }
        public virtual DbSet<ControlStationPage> ControlStationPages { get; set; }
        public virtual DbSet<ControlStationRail> ControlStationRails { get; set; }
        public virtual DbSet<ControlStationResponseModule> ControlStationResponseModules { get; set; }
        public virtual DbSet<ControlStationRoute> ControlStationRoutes { get; set; }
        public virtual DbSet<ControlStationRouteList> ControlStationRouteLists { get; set; }
        public virtual DbSet<DcFunction> DcFunctions { get; set; }
        public virtual DbSet<Function> Functions { get; set; }
        public virtual DbSet<LayoutDatum> LayoutData { get; set; }
        public virtual DbSet<TractionList> TractionLists { get; set; }
        public virtual DbSet<TrainList> TrainLists { get; set; }
        public virtual DbSet<UpdateHistory> UpdateHistories { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }
        public virtual DbSet<VehiclesToCategory> VehiclesToCategories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=Loco.sqlite");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AndroidMetadatum>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("android_metadata");

                entity.Property(e => e.Locale).HasColumnName("locale");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Name).HasColumnName("name");
            });

            modelBuilder.Entity<ControlStationControl>(entity =>
            {
                entity.ToTable("control_station_controls");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Address1).HasColumnName("address1");

                entity.Property(e => e.Address2).HasColumnName("address2");

                entity.Property(e => e.Address3).HasColumnName("address3");

                entity.Property(e => e.Angle).HasColumnName("angle");

                entity.Property(e => e.ButtonType)
                    .HasColumnName("button_type")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.PageId).HasColumnName("page_id");

                entity.Property(e => e.Time)
                    .HasColumnName("time")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.X).HasColumnName("x");

                entity.Property(e => e.Y).HasColumnName("y");
            });

            modelBuilder.Entity<ControlStationControlState>(entity =>
            {
                entity.ToTable("control_station_control_states");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Address1Value).HasColumnName("address1_value");

                entity.Property(e => e.Address2Value).HasColumnName("address2_value");

                entity.Property(e => e.Address3Value).HasColumnName("address3_value");

                entity.Property(e => e.ControlId).HasColumnName("control_id");

                entity.Property(e => e.State).HasColumnName("state");
            });

            modelBuilder.Entity<ControlStationImage>(entity =>
            {
                entity.ToTable("control_station_images");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.ImageName).HasColumnName("image_name");

                entity.Property(e => e.PageId).HasColumnName("page_id");
            });

            modelBuilder.Entity<ControlStationNote>(entity =>
            {
                entity.ToTable("control_station_notes");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Angle)
                    .HasColumnName("angle")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.FontSize).HasColumnName("font_size");

                entity.Property(e => e.PageId).HasColumnName("page_id");

                entity.Property(e => e.Text).HasColumnName("text");

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasDefaultValueSql("1");

                entity.Property(e => e.X).HasColumnName("x");

                entity.Property(e => e.Y).HasColumnName("y");
            });

            modelBuilder.Entity<ControlStationPage>(entity =>
            {
                entity.ToTable("control_station_pages");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.Position).HasColumnName("position");

                entity.Property(e => e.Thumb).HasColumnName("thumb");
            });

            modelBuilder.Entity<ControlStationRail>(entity =>
            {
                entity.ToTable("control_station_rails");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.LeftControlId).HasColumnName("left_control_id");

                entity.Property(e => e.LeftOutlet).HasColumnName("left_outlet");

                entity.Property(e => e.LeftResponseModuleId)
                    .HasColumnName("left_response_module_id")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.PageId).HasColumnName("page_id");

                entity.Property(e => e.RightControlId).HasColumnName("right_control_id");

                entity.Property(e => e.RightOutlet).HasColumnName("right_outlet");

                entity.Property(e => e.RightResponseModuleId)
                    .HasColumnName("right_response_module_id")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.Value).HasColumnName("value");
            });

            modelBuilder.Entity<ControlStationResponseModule>(entity =>
            {
                entity.ToTable("control_station_response_modules");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Address).HasColumnName("address");

                entity.Property(e => e.Afterglow).HasColumnName("afterglow");

                entity.Property(e => e.Angle).HasColumnName("angle");

                entity.Property(e => e.PageId).HasColumnName("page_id");

                entity.Property(e => e.ReportAddress).HasColumnName("report_address");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.X).HasColumnName("x");

                entity.Property(e => e.Y).HasColumnName("y");
            });

            modelBuilder.Entity<ControlStationRoute>(entity =>
            {
                entity.ToTable("control_station_routes");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Angle).HasColumnName("angle");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.PageId).HasColumnName("page_id");

                entity.Property(e => e.X).HasColumnName("x");

                entity.Property(e => e.Y).HasColumnName("y");
            });

            modelBuilder.Entity<ControlStationRouteList>(entity =>
            {
                entity.ToTable("control_station_route_list");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.ControlId).HasColumnName("control_id");

                entity.Property(e => e.Position).HasColumnName("position");

                entity.Property(e => e.RouteId).HasColumnName("route_id");

                entity.Property(e => e.StateId).HasColumnName("state_id");

                entity.Property(e => e.WaitTime).HasColumnName("wait_time");
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

            modelBuilder.Entity<UpdateHistory>(entity =>
            {
                entity.ToTable("update_history");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.BuildNumber).HasColumnName("build_number");

                entity.Property(e => e.BuildVersion).HasColumnName("build_version");

                entity.Property(e => e.Os).HasColumnName("os");

                entity.Property(e => e.ToDatabaseVersion).HasColumnName("to_database_version");

                entity.Property(e => e.UpdateDate).HasColumnName("update_date");
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

                entity.Property(e => e.PanoramaImage).HasColumnName("panoramaImage");

                entity.Property(e => e.PanoramaWidth).HasColumnName("panorama_width");

                entity.Property(e => e.PanoramaX).HasColumnName("panorama_x");

                entity.Property(e => e.PanoramaY).HasColumnName("panorama_y");

                entity.Property(e => e.Position).HasColumnName("position");

                entity.Property(e => e.Railway).HasColumnName("railway");

                entity.Property(e => e.Rmin).HasColumnName("rmin");

                entity.Property(e => e.ServiceWeight).HasColumnName("service_weight");

                entity.Property(e => e.SpeedDisplay).HasColumnName("speed_display");

                entity.Property(e => e.TractionDirection).HasColumnName("traction_direction");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.Video).HasColumnName("video");

                entity.Property(e => e.VideoWidth).HasColumnName("video_width");

                entity.Property(e => e.VideoX).HasColumnName("video_x");

                entity.Property(e => e.VideoY).HasColumnName("video_y");
            });

            modelBuilder.Entity<VehiclesToCategory>(entity =>
            {
                entity.ToTable("vehicles_to_categories");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.CategoryId).HasColumnName("category_id");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
