using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Model;
using System;
using System.Linq;
using WPF_Application.Extensions;

#nullable disable

namespace WPF_Application.Infrastructure
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
            modelBuilder.Entity<Vehicle>().HasOne(e => e.Category);
            modelBuilder.Entity<Vehicle>().HasMany(e => e.Functions);
            modelBuilder.Entity<Category>().HasMany(e => e.Vehicles);

            var converter = new ValueConverter<decimal?[], string>(v => string.Join(";", v), v => v.Split(";", StringSplitOptions.None).Select(val => val.IsDecimal() ? decimal.Parse(val) : (decimal?)null).ToArray());
            modelBuilder.Entity<Vehicle>().Property(e => e.TractionForward).HasConversion(converter);
            modelBuilder.Entity<Vehicle>().Property(e => e.TractionBackward).HasConversion(converter);
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);


        public override EntityEntry<TEntity> Add<TEntity>(TEntity obj) where TEntity : class
        {
            var result = Set<TEntity>().Add(obj);

            try
            {
                SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ApplicationException($"Bei der Operation ist ein fehler aufgetreten.");
            }
            catch (DbUpdateException)
            {
                throw new ApplicationException($"Bei der Operation ist ein fehler aufgetreten.");
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public override EntityEntry<TEntity> Remove<TEntity>(TEntity obj) where TEntity : class
        {
            var value = Set<TEntity>().Remove(obj);
            try
            {
                SaveChanges();
            }
            catch (DbUpdateException)
            {
                throw new ApplicationException("Bei der Operation ist ein fehler aufgetreten.");
            }
            catch (Exception)
            {
                throw;
            }
            return value;
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity obj) where TEntity : class
        {
            var result = Set<TEntity>().Update(obj);
            try
            {
                SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ApplicationException($"Bei der Operation ist ein fehler aufgetreten.");
            }
            catch (DbUpdateException)
            {
                throw new ApplicationException($"Bei der Operation ist ein fehler aufgetreten.");
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }
    }
}
