using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Quarks.DataAccess.EntityFramework.ContextManagement;
using Quarks.Transactions;

namespace Quarks.DataAccess.EntityFramework
{
	internal static class EfTransaction
	{
		private static readonly object CurrentLock = new object();

		public static EfTransaction<TDbContext> GetCurrent<TDbContext>(IEfContextManager<TDbContext> contextManager) where TDbContext : DbContext
		{
			if (Transaction.Current == null)
			{
				return new EfTransaction<TDbContext>(contextManager);
			}

			string key = contextManager.GetHashCode().ToString();

			IDependentTransaction current;
			if (!Transaction.Current.DependentTransactions.TryGetValue(key, out current))
			{
				lock (CurrentLock)
				{
					if (!Transaction.Current.DependentTransactions.TryGetValue(key, out current))
					{
						current = new EfTransaction<TDbContext>(contextManager);
						Transaction.Current.Enlist(key, current);
					}
				}
			}

			return (EfTransaction<TDbContext>)current;
		}
	}

	public class EfTransaction<TDbContext> : IDependentTransaction where TDbContext : DbContext
	{
		private bool _disposed;
		private readonly Lazy<TDbContext> _context;

		internal EfTransaction(IEfContextManager<TDbContext> contextManager)
		{
			if (contextManager == null) throw new ArgumentNullException(nameof(contextManager));

			ContextManager = contextManager;
			_context = new Lazy<TDbContext>(contextManager.CreateContext);
		}

		public IEfContextManager<TDbContext> ContextManager { get; }

		public TDbContext Context => _context.Value;

		public void Dispose()
		{
			ThrowIfDisposed();

			if (_context.IsValueCreated)
			{
				Context.Dispose();
			}

			_disposed = true;
		}

		public Task CommitAsync(CancellationToken cancellationToken)
		{
			ThrowIfDisposed();

			return _context.IsValueCreated 
				? Context.SaveChangesAsync(cancellationToken) 
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