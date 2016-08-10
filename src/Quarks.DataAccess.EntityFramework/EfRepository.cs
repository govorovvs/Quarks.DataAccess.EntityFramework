using Microsoft.EntityFrameworkCore;
using Quarks.DataAccess.EntityFramework.ContextManagement;

namespace Quarks.DataAccess.EntityFramework
{
	public abstract class EfRepository<TDbContext> where TDbContext : DbContext
	{
		private readonly IEfContextManager<TDbContext> _contextManager;

		protected EfRepository(IEfContextManager<TDbContext> contextManager)
		{
			_contextManager = contextManager;
		}

		protected TDbContext Context
		{
			get { return UnitOfWork.Context; }
		}

		private EfTransaction<TDbContext> UnitOfWork
		{
			get { return EfTransaction.GetCurrent(_contextManager); }
		}
	}
}