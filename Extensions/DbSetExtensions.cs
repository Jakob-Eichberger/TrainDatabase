using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
    public static class DbSetExtensions
    {
        public static void RemoveAll<TEntity>(this DbSet<TEntity> e) where TEntity : class
        {
            foreach (var item in e.ToList())
            {
                e.Remove(item);
            }
        }
    }
}
