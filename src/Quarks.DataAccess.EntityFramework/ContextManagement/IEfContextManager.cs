using Microsoft.EntityFrameworkCore;

namespace Quarks.DataAccess.EntityFramework.ContextManagement
{
	public interface IEfContextManager<out TDbContext> where TDbContext : DbContext
	{
		TDbContext CreateContext();

		int GetHashCode();
	}
}