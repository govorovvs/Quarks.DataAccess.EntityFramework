using Microsoft.EntityFrameworkCore;

namespace Quarks.DataAccess.EntityFramework
{
    public interface IEfTransaction<out TDbContext> where TDbContext : DbContext
    {
        TDbContext Context { get; }
    }
}