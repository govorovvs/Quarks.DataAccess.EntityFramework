using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Quarks.DataAccess.EntityFramework.ContextManagement;
using Quarks.DomainModel.Impl;

namespace Quarks.DataAccess.EntityFramework
{
	internal static class EfUnitOfWork
	{
		private static readonly object CurrentLock = new object();

		public static EfUnitOfWork<TDbContext> GetCurrent<TDbContext>(IEfContextManager<TDbContext> contextManager) where TDbContext : DbContext
		{
			if (UnitOfWork.Current == null)
			{
				return new EfUnitOfWork<TDbContext>(contextManager);
			}

			string key = contextManager.GetHashCode().ToString();

			IDependentUnitOfWork current;
			if (!UnitOfWork.Current.DependentUnitOfWorks.TryGetValue(key, out current))
			{
				lock (CurrentLock)
				{
					if (!UnitOfWork.Current.DependentUnitOfWorks.TryGetValue(key, out current))
					{
						current = new EfUnitOfWork<TDbContext>(contextManager);
						UnitOfWork.Current.Enlist(key, current);
					}
				}
			}

			return (EfUnitOfWork<TDbContext>)current;
		}
	}

	public class EfUnitOfWork<TDbContext> : IDependentUnitOfWork where TDbContext : DbContext
	{
		private bool _disposed;
		private readonly Lazy<TDbContext> _context;

		internal EfUnitOfWork(IEfContextManager<TDbContext> contextManager)
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
				: Task.CompletedTask;
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