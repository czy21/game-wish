using Microsoft.EntityFrameworkCore;
using Sunny.Framework.DB.Repository;
using WishServer.Domain;
using WishServer.Model.DTO;

namespace WishServer.Repository.impl
{
    public class GameRoomRepository(AppDbContext dbContext) : RepositoryBase<long, GameRoomPO>(dbContext), IGameRoomRepository
    {
        public async Task<GameRoomDTO?> AggRoom(string roomId)
        {
            var query = from r in dbContext.GameRooms
                        join d in dbContext.GameRounds
                            on new { r.GameId, r.RoomId } equals new { d.GameId, d.RoomId } into tempJoin
                        from rightItem in tempJoin.DefaultIfEmpty()
                        where r.RoomId == roomId
                        group r by new{r.GameId, r.RoomId} into g
                        select new GameRoomDTO()
                        {
                            GameId = g.Key.GameId,
                            RoomId = g.Key.RoomId,
                            RoundCount = g.Count(),
                        };
            return await query.FirstOrDefaultAsync();
        }
    }
}