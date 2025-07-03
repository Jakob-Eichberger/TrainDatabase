using Extensions;
using Helper;
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
    public Database()
    {
    }

    public Database(DbContextOptions<Database> options) : base(options)
    {
    }

    public event EventHandler CollectionChanged;

    public virtual DbSet<FunctionModel> Functions => Set<FunctionModel>();

    public virtual DbSet<VehicleModel> Vehicles => Set<VehicleModel>();

    override public EntityEntry<TEntity> Add<TEntity>(TEntity obj) where TEntity : class
    {
      EntityEntry<TEntity> result = Set<TEntity>().Add(obj);
      SaveChanges();
      CollectionChanged?.Invoke(this, new());
      return result;
    }

    public async Task<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity obj) where TEntity : class
    {
      EntityEntry<TEntity> result = await Set<TEntity>().AddAsync(obj);
      await SaveChangesAsync();
      CollectionChanged?.Invoke(this, new());
      return result;
    }

    public void InvokeCollectionChanged()
    {
      CollectionChanged?.Invoke(this, new());
    }

    override public EntityEntry<TEntity> Remove<TEntity>(TEntity obj) where TEntity : class
    {
      EntityEntry<TEntity> value = Set<TEntity>().Remove(obj);
      SaveChanges();
      CollectionChanged?.Invoke(this, new());
      return value;
    }

    public async Task<EntityEntry<TEntity>> RemoveAsync<TEntity>(TEntity obj) where TEntity : class
    {
      EntityEntry<TEntity> value = Set<TEntity>().Remove(obj);
      await SaveChangesAsync();
      CollectionChanged?.Invoke(this, new());
      return value;
    }

    public void RemoveRange<TEntity>(List<TEntity> obj) where TEntity : class
    {
      Set<TEntity>().RemoveRange(obj);
      SaveChanges();
      CollectionChanged?.Invoke(this, new());
    }

    override public EntityEntry<TEntity> Update<TEntity>(TEntity obj) where TEntity : class
    {
      EntityEntry<TEntity> result = Set<TEntity>().Update(obj);
      SaveChanges();
      CollectionChanged?.Invoke(this, new());
      return result;
    }

    public async Task<EntityEntry<TEntity>> UpdateAsync<TEntity>(TEntity obj) where TEntity : class
    {
      EntityEntry<TEntity> result = Set<TEntity>().Update(obj);
      await SaveChangesAsync();
      CollectionChanged?.Invoke(this, new());
      return result;
    }

    public void UpdateRange<TEntity>(List<TEntity> obj) where TEntity : class
    {
      Set<TEntity>().UpdateRange(obj);
      SaveChanges();
      CollectionChanged?.Invoke(this, new());
    }

    public async Task UpdateRangeAsync<TEntity>(List<TEntity> obj) where TEntity : class
    {
      Set<TEntity>().UpdateRange(obj);
      await SaveChangesAsync();
      CollectionChanged?.Invoke(this, new());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      string dbPath = Configuration.ApplicationData.DatabaseFile.FullName;
      Directory.CreateDirectory(Path.GetDirectoryName(dbPath));
      if (!optionsBuilder.IsConfigured)
      {
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
      }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      ValueConverter<decimal?[], string> decimArrayToStringConverter = new(
                                                                           v => string.Join(";", v),
                                                                           v => v.Split(";", StringSplitOptions.None)
                                                                                 .Select(
                                                                                         val => val.IsDecimal()
                                                                                                  ? decimal.Parse(val)
                                                                                                  : (decimal?)null)
                                                                                 .ToArray());
      ValueComparer<decimal?[]> decimalArrayValueComparer = new(
                                                                (a, b) => a.SequenceEqual(b),
                                                                v => v.Aggregate(
                                                                                 0,
                                                                                 (a, i) => HashCode.Combine(
                                                                                                            a,
                                                                                                            i.GetHashCode())),
                                                                v => v.ToArray());
      modelBuilder.Entity<VehicleModel>().Property(e => e.TractionForward).HasConversion(decimArrayToStringConverter)
                  .Metadata.SetValueComparer(decimalArrayValueComparer);
      modelBuilder.Entity<VehicleModel>().Property(e => e.TractionBackward).HasConversion(decimArrayToStringConverter)
                  .Metadata.SetValueComparer(decimalArrayValueComparer);

      ValueConverter<List<int>, string> intListConverter = new(
                                                               v => string.Join(";", v.Distinct()),
                                                               v => v.Split(";", StringSplitOptions.RemoveEmptyEntries)
                                                                     .Select(
                                                                             val => val.IsInt()
                                                                                      ? int.Parse(val)
                                                                                      : int.MinValue).Distinct()
                                                                     .ToList());
      ValueComparer<List<int>> intListValueComparer = new(
                                                          (a, b) => a.SequenceEqual(b),
                                                          v => v.Aggregate(
                                                                           0,
                                                                           (a, i) => HashCode.Combine(
                                                                                                      a,
                                                                                                      i.GetHashCode())),
                                                          v => v.ToList());
      modelBuilder.Entity<VehicleModel>().Property(e => e.TractionVehicleIds).HasConversion(intListConverter).Metadata
                  .SetValueComparer(intListValueComparer);

      OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    /// <summary>
    /// Deletes all dbset data.
    /// </summary>
    public void DeleteAll()
    {
      DetachAllEntities();
      Functions.RemoveAll();
      Vehicles.RemoveAll();
      SaveChanges();
      InvokeCollectionChanged();
    }

    /// <summary>
    /// Detaches all tracked entities
    /// </summary>
    public void DetachAllEntities()
    {
      List<EntityEntry> changedEntriesCopy = ChangeTracker.Entries()
                                                          .Where(
                                                                 e => e.State is EntityState.Added
                                                                              or EntityState.Modified
                                                                              or EntityState.Deleted).ToList();
      foreach (EntityEntry entry in changedEntriesCopy)
      {
        entry.State = EntityState.Detached;
      }
    }
  }
}