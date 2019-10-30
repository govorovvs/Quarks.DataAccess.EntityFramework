using System;
using System.Threading;
using System.Threading.Tasks;
using Quarks.Transactions;
#if NET461
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
#endif

namespace Quarks.DataAccess.EntityFramework
{
	public static class EfTransaction
	{
	    public static IEfTransaction<TDbContext> GetCurrent<TDbContext>(IDbContextFactory<TDbContext> contextFactory)
	        where TDbContext : DbContext
	    {
	        Transaction transaction = Transaction.Current;
	        string key = contextFactory.GetKey();

	        IDependentTransaction current =
	            transaction == null
	                ? new EfTransaction<TDbContext>(contextFactory)
	                : transaction.GetOrEnlist(key, () => new EfTransaction<TDbContext>(contextFactory));
	        return (EfTransaction<TDbContext>) current;
	    }
	}

    internal class EfTransaction<TDbContext> : IEfTransaction<TDbContext>, IDependentTransaction where TDbContext : DbContext
	{
		private bool _disposed;
		private TDbContext _context;

	    internal EfTransaction(IDbContextFactory<TDbContext> contextFactory)
		{
			if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));

		    ContextFactory = contextFactory;
		}

        public IDbContextFactory<TDbContext> ContextFactory { get; }

        public TDbContext Context
	    {
	        get { return _context ?? (_context = ContextFactory.Create()); }
	    }

		public void Dispose()
		{
			ThrowIfDisposed();

            _context ?.Dispose();
		    _context = null;

			_disposed = true;
		}

		public Task CommitAsync(CancellationToken cancellationToken)
		{
			ThrowIfDisposed();

			return _context != null
				? _context.SaveChangesAsync(cancellationToken) 
				: Task.FromResult(0);
		}

		private void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}
	}
}