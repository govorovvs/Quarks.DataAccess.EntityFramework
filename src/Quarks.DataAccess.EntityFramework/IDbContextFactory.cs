using Microsoft.EntityFrameworkCore;

namespace Quarks.DataAccess.EntityFramework
{
    public interface IDbContextFactory<out TDbContext> where TDbContext : DbContext
    {
        TDbContext Create();

        string GetKey();
    }
}