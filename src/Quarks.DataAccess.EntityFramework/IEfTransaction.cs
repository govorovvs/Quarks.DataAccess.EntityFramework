#if NET461
using System.Data.Entity;
#else
using Microsoft.EntityFrameworkCore;
#endif

namespace Quarks.DataAccess.EntityFramework
{
    public interface IEfTransaction<out TDbContext> where TDbContext : DbContext
    {
        TDbContext Context { get; }
    }
}