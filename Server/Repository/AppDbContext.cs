using Microsoft.EntityFrameworkCore;
using WishServer.Domain;

namespace WishServer.Repository;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<ConfigPO> Config { get; set; }

    public DbSet<WishUserPO> WishUser { get; set; }

    public DbSet<WishItemPO> WishItem { get; set; }

    public DbSet<GameUserPO> GameUser { get; set; }
    public DbSet<GameGiftPO> GameGift { get; set; }
}