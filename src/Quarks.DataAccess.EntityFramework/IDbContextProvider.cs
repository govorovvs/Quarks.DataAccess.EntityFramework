using Microsoft.EntityFrameworkCore;

namespace Quarks.DataAccess.EntityFramework
{
    public interface IDbContextProvider<out TDbContext> where TDbContext : DbContext
    {
        TDbContext Context { get; }
    }
}