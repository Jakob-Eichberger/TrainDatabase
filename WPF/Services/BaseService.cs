using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WPF_Application.Infrastructure;

namespace WPF_Application.Services
{
    public class BaseService
    {
        protected readonly Database _db;

        public BaseService(Database db)
        {
            _db = db;
        }

        public IQueryable<TEntity> GetTable<TEntity>() where TEntity : class => _db.Set<TEntity>();

        protected void AddAsync<TEntity>(TEntity obj) where TEntity : class
        {
            _db.Set<TEntity>().Add(obj);

            try
            {
                _db.SaveChanges();
            }
            // Datensatz ist nicht mehr vorhanden.
            catch (DbUpdateConcurrencyException)
            {
                //Logger functionality
                throw new ApplicationException($"Bei der Operation ist ein fehler aufgetreten.");
            }
            catch (DbUpdateException)
            {
                //Logger functionality
                throw new ApplicationException($"Bei der Operation ist ein fehler aufgetreten.");
            }
            catch (Exception)
            {
                //_logger.LogError(e, $"An Error occurued in the Add<> Method of the AddClase");
                throw;
            }
        }

        protected void Delete<TEntity>(TEntity obj) where TEntity : class
        {
            _db.Set<TEntity>().Remove(obj);

            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                throw new ApplicationException("Bei der Operation ist ein fehler aufgetreten.");
            }
            catch (Exception)
            {
                //_logger.LogError(e, $"Beim Löschen ist ein fehler aufgetreten!");
                throw;
            }

        }

        protected void Update<TEntity>(TEntity obj) where TEntity : class
        {
            _db.Set<TEntity>().Update(obj);
            try
            {
                _db.SaveChanges();
            }
            // Datensatz ist nicht mehr vorhanden.
            catch (DbUpdateConcurrencyException)
            {
                //Logger functionality
                throw new ApplicationException($"Bei der Operation ist ein fehler aufgetreten.");
            }
            catch (DbUpdateException)
            {
                //Logger functionality
                throw new ApplicationException($"Bei der Operation ist ein fehler aufgetreten.");
            }
            catch (Exception)
            {
                //Logger functionality
                throw;
            }
        }

    }
}
