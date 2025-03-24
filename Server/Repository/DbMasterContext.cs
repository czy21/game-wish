using Microsoft.EntityFrameworkCore;
using WishServer.Domain;

namespace WishServer.Repository;

public class DbMasterContext : DbContext
{
    public DbMasterContext(DbContextOptions<DbMasterContext> options) : base(options)
    {
        
    }

    public DbSet<ConfigPO> Config { get; set; }

    public DbSet<WishUserPO> WishUser { get; set; }

    public DbSet<WishItemPO> WishItem { get; set; }
}