using Microsoft.EntityFrameworkCore;
using WishServer.Domain;

namespace Demo.Repository;

public class DbMasterContext : DbContext
{
    public DbMasterContext(DbContextOptions<DbMasterContext> options) : base(options)
    {
    }

    public DbSet<WishUserPO?> WishUsers { get; set; }
}