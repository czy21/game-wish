using Microsoft.EntityFrameworkCore;
using WishServer.Domain;

namespace WishServer.Repository;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ConfigPO> Configs { get; set; }
    public DbSet<GamePO> Games { get; set; }
    public DbSet<GameAppPO> GameApps { get; set; }
    public DbSet<GameGiftPO> GameGifts { get; set; }
    public DbSet<GameRoomPO> GameRooms { get; set; }
    public DbSet<GameRoundPO> GameRounds { get; set; }
    public DbSet<GameUserPO> GameUsers { get; set; }
    public DbSet<WishItemPO> WishItems { get; set; }
    public DbSet<WishUserPO> WishUsers { get; set; }
}