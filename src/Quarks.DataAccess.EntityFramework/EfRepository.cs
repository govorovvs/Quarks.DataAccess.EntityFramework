using System;
#if NET461
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
#endif

namespace Quarks.DataAccess.EntityFramework
{
	public abstract class EfRepository<TDbContext> where TDbContext : DbContext
	{
	    private readonly IDbContextProvider<TDbContext> _contextProvider;

	    protected EfRepository(IDbContextProvider<TDbContext> contextProvider)
	    {
            if (contextProvider == null) throw new ArgumentNullException(nameof(contextProvider));

	        _contextProvider = contextProvider;
	    }

        protected TDbContext Context
		{
			get { return _contextProvider.Context; }
		}
	}
}