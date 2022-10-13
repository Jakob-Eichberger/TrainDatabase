using Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace Infrastructure
{
    public partial class Database : DbContext
    {
        public Database() { }

        public Database(DbContextOptions<Database> options) : base(options) { }

        public event EventHandler CollectionChanged;

        public virtual DbSet<FunctionModel> Functions => Set<FunctionModel>();

        public virtual DbSet<VehicleModel> Vehicles => Set<VehicleModel>();

        public override EntityEntry<TEntity> Add<TEntity>(TEntity obj) where TEntity : class
        {
            var result = Set<TEntity>().Add(obj);
            SaveChanges();
            CollectionChanged?.Invoke(this, new EventArgs());
            return result;
        }

        public async Task<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity obj) where TEntity : class
        {
            var result = await Set<TEntity>().AddAsync(obj);
            await SaveChangesAsync();
            CollectionChanged?.Invoke(this, new EventArgs());
            return result;
        }

        public void InvokeCollectionChanged() => CollectionChanged?.Invoke(this, new EventArgs());

        public override EntityEntry<TEntity> Remove<TEntity>(TEntity obj) where TEntity : class
        {
            var value = Set<TEntity>().Remove(obj);
            SaveChanges();
            CollectionChanged?.Invoke(this, new EventArgs());
            return value;
        }

        public async Task<EntityEntry<TEntity>> RemoveAsync<TEntity>(TEntity obj) where TEntity : class
        {
            var value = Set<TEntity>().Remove(obj);
            await SaveChangesAsync();
            CollectionChanged?.Invoke(this, new EventArgs());
            return value;
        }

        public void RemoveRange<TEntity>(List<TEntity> obj) where TEntity : class
        {
            Set<TEntity>().RemoveRange(obj);
            SaveChanges();
            CollectionChanged?.Invoke(this, new EventArgs());
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity obj) where TEntity : class
        {
            var result = Set<TEntity>().Update(obj);
            SaveChanges();
            CollectionChanged?.Invoke(this, new EventArgs());
            return result;
        }

        public async Task<EntityEntry<TEntity>> UpdateAsync<TEntity>(TEntity obj) where TEntity : class
        {
            var result = Set<TEntity>().Update(obj);
            await SaveChangesAsync();
            CollectionChanged?.Invoke(this, new EventArgs());
            return result;
        }

        public void UpdateRange<TEntity>(List<TEntity> obj) where TEntity : class
        {
            Set<TEntity>().UpdateRange(obj);
            SaveChanges();
            CollectionChanged?.Invoke(this, new EventArgs());
        }

        public async Task UpdateRangeAsync<TEntity>(List<TEntity> obj) where TEntity : class
        {
            Set<TEntity>().UpdateRange(obj);
            await SaveChangesAsync();
            CollectionChanged?.Invoke(this, new EventArgs());
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var path = Path.Combine(new string[] { Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TrainDb", "Loco.sqlite" });
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlite($"Data Source={path}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var converter = new ValueConverter<decimal?[], string>(v => string.Join(";", v), v => v.Split(";", StringSplitOptions.None).Select(val => val.IsDecimal() ? decimal.Parse(val) : (decimal?)null).ToArray());
            modelBuilder.Entity<VehicleModel>().Property(e => e.TractionForward).HasConversion(converter);
            modelBuilder.Entity<VehicleModel>().Property(e => e.TractionBackward).HasConversion(converter);
            modelBuilder.Entity<VehicleModel>().Property(e => e.TractionVehicleIds).HasConversion(new ValueConverter<List<int>, string>(v => string.Join(";", v.Distinct()), v => v.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(val => val.IsInt() ? int.Parse(val) : int.MinValue).Distinct().ToList()));
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        /// <summary>
        /// Deletes all dbset data.
        /// </summary>
        public void Clear()
        {
            Functions.RemoveAll();
            Vehicles.RemoveAll();
            SaveChanges();
            InvokeCollectionChanged();
        }
    }
}
