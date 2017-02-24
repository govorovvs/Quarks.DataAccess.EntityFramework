using System;
using Microsoft.EntityFrameworkCore;

namespace Quarks.DataAccess.EntityFramework
{
    public class DbContextProvider<TDbContext> : IDbContextProvider<TDbContext> where TDbContext : DbContext
    {
        private readonly IDbContextFactory<TDbContext> _contextFactory;

        public DbContextProvider(IDbContextFactory<TDbContext> contextFactory)
        {
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));

            _contextFactory = contextFactory;
        }

        public virtual TDbContext Context
        {
            get { return Transaction.Context; }
        }

        private IEfTransaction<TDbContext> Transaction
        {
            get { return EfTransaction.GetCurrent(_contextFactory); }
        }
    }
}