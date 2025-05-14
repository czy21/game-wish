using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Sunny.Framework.DB.Repository;
using System.Data;
using WishServer.Domain;
using WishServer.Model.DTO;

namespace WishServer.Repository.impl
{
    public class GameRoomRepository(AppDbContext dbContext, IMapper mapper) : RepositoryBase<long?, GameRoomPO>(dbContext), IGameRoomRepository
    {
        AppDbContext _dbContext = dbContext;
        IMapper _mapper = mapper;

        public async Task<GameRoomDTO> AggRoom(string roomId)
        {
            var query = from r in _dbContext.GameRooms
                        join d in _dbContext.GameRounds
                            on new { r.GameId, r.RoomId } equals new { d.GameId, d.RoomId } into tempJoin
                        from rightItem in tempJoin.DefaultIfEmpty()
                        where r.RoomId == roomId
                        group r by new { r.GameId, r.RoomId } into g
                        select new GameRoomDTO()
                        {
                            GameId = g.Key.GameId,
                            RoomId = g.Key.RoomId,
                            RoundCount = g.Count(),
                        };
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<GameRoomDTO>> AggRoom1(string roomId)
        {
            //var a = from room in _dbContext.GameRooms
            //        join round in _dbContext.GameRounds
            //          on room.RoomId equals round.RoomId into rounds
            //        select new GameRoomDTO
            //        {
            //            RoomId = room.RoomId,
            //            GameId = room.GameId,
            //            Rounds = rounds.Select(t => new GameRoundPO { RoundId = t.RoundId }).ToList(),
            //            RoundCount = rounds.Count()
            //        };
            //return await a.ToListAsync();
            return await AggRom2(roomId);
        }

        public async Task<List<GameRoomDTO>> AggRom2(string roomId)
        {

            var conn = _dbContext.Database.GetDbConnection();

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            var roomDict = new Dictionary<string, GameRoomDTO>();

            string sql = @"
select
  a.*,
  b.*
from game_room a
left join game_round b on a.room_id = b.room_id
order by a.room_id,b.round_id
";
            _ = (await conn.QueryAsync<GameRoomPO, GameRoundPO, GameRoomDTO>(sql, (room, round) =>
            {
                if (!roomDict.TryGetValue(room.RoomId ?? "", out var current))
                {
                    current = _mapper.Map<GameRoomDTO>(room);
                    current.Rounds = [];
                    roomDict.Add(room.RoomId ?? "", current);
                }
                if (round != null)
                {
                    current.Rounds.Add(round);
                }
                current.RoundCount = current.Rounds.Count();
                return current;
            }, splitOn: "round_id"));
            return [.. roomDict.Values];
        }
    }
}
