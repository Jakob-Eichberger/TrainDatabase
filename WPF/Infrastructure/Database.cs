using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrainDatabase.Extensions;

#nullable disable

namespace TrainDatabase.Infrastructure
{
    public partial class Database : DbContext
    {
        public event EventHandler CollectionChanged;

        public Database()
        {
        }

        public Database(DbContextOptions<Database> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories => Set<Category>();

        public virtual DbSet<DcFunction> DcFunctions => Set<DcFunction>();

        public virtual DbSet<Function> Functions => Set<Function>();

        public virtual DbSet<LayoutDatum> LayoutData => Set<LayoutDatum>();

        public virtual DbSet<TractionList> TractionLists => Set<TractionList>();

        public virtual DbSet<TrainList> TrainLists => Set<TrainList>();

        public virtual DbSet<Vehicle> Vehicles => Set<Vehicle>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=Loco.sqlite");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Vehicle>().HasOne(e => e.Category);
            //modelBuilder.Entity<Vehicle>().HasMany(e => e.Functions);
            //modelBuilder.Entity<Category>().HasMany(e => e.Vehicles);

            var converter = new ValueConverter<decimal?[], string>(v => string.Join(";", v), v => v.Split(";", StringSplitOptions.None).Select(val => val.IsDecimal() ? decimal.Parse(val) : (decimal?)null).ToArray());
            modelBuilder.Entity<Vehicle>().Property(e => e.TractionForward).HasConversion(converter);
            modelBuilder.Entity<Vehicle>().Property(e => e.TractionBackward).HasConversion(converter);
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public override EntityEntry<TEntity> Add<TEntity>(TEntity obj) where TEntity : class
        {
            var result = Set<TEntity>().Add(obj);
            SaveChanges();
            if (CollectionChanged is not null) CollectionChanged.Invoke(this, new EventArgs());
            return result;
        }

        public async Task<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity obj) where TEntity : class
        {
            var result = await Set<TEntity>().AddAsync(obj);
            await SaveChangesAsync();
            if (CollectionChanged is not null) CollectionChanged.Invoke(this, new EventArgs());
            return result;
        }


        public override EntityEntry<TEntity> Remove<TEntity>(TEntity obj) where TEntity : class
        {
            var value = Set<TEntity>().Remove(obj);
            SaveChanges();
            if (CollectionChanged is not null) CollectionChanged.Invoke(this, new EventArgs());
            return value;
        }


        public async Task<EntityEntry<TEntity>> RemoveAsync<TEntity>(TEntity obj) where TEntity : class
        {
            var value = Set<TEntity>().Remove(obj);
            await SaveChangesAsync();
            if (CollectionChanged is not null) CollectionChanged.Invoke(this, new EventArgs());
            return value;
        }

        public void RemoveRange<TEntity>(List<TEntity> obj) where TEntity : class
        {
            Set<TEntity>().RemoveRange(obj);
            SaveChanges();
            if (CollectionChanged is not null) CollectionChanged.Invoke(this, new EventArgs());
        }


        public async Task UpdateRangeAsync<TEntity>(List<TEntity> obj) where TEntity : class
        {
            Set<TEntity>().UpdateRange(obj);
            await SaveChangesAsync();
            if (CollectionChanged is not null) CollectionChanged.Invoke(this, new EventArgs());
        }

        public void UpdateRange<TEntity>(List<TEntity> obj) where TEntity : class
        {
            Set<TEntity>().UpdateRange(obj);
            SaveChanges();
            if (CollectionChanged is not null) CollectionChanged.Invoke(this, new EventArgs());
        }


        public async Task<EntityEntry<TEntity>> UpdateAsync<TEntity>(TEntity obj) where TEntity : class
        {
            var result = Set<TEntity>().Update(obj);
            await SaveChangesAsync();
            if (CollectionChanged is not null) CollectionChanged.Invoke(this, new EventArgs());
            return result;
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity obj) where TEntity : class
        {
            var result = Set<TEntity>().Update(obj);
            SaveChanges();
            if (CollectionChanged is not null) CollectionChanged.Invoke(this, new EventArgs());
            return result;
        }

        public void InvokeCollectionChanged()
        {
            if (CollectionChanged is not null) CollectionChanged.Invoke(this, new EventArgs());
        }
    }
}
