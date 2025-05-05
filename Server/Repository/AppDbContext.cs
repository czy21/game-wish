using System;
using System.Collections.Generic;
using Dapper.FluentMap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WishServer.Domain;


namespace WishServer.Repository
{
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
        public DbSet<GameGiftPO> GameGifts { get; set; }
        public DbSet<GameRoomPO> GameRooms { get; set; }
        public DbSet<GameRoundPO> GameRounds { get; set; }
        public DbSet<GameUserPO> GameUsers { get; set; }
        public DbSet<WishItemPO> WishItems { get; set; }
        public DbSet<WishUserPO> WishUsers { get; set; }

        public static void InitMap()
        {
            FluentMapper.Initialize(t =>
            {
                t.AddMap(new ConfigPOMap());
                t.AddMap(new GamePOMap());
                t.AddMap(new GameGiftPOMap());
                t.AddMap(new GameRoomPOMap());
                t.AddMap(new GameRoundPOMap());
                t.AddMap(new GameUserPOMap());
                t.AddMap(new WishItemPOMap());
                t.AddMap(new WishUserPOMap());
            });
        }
    }
}
