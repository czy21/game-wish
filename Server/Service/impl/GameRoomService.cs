using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WishServer.Domain;
using WishServer.Model.DTO;
using WishServer.Repository;

namespace WishServer.Service.impl;

public class GameRoomService : ServiceBase, IGameRoomService
{
    private readonly IGameRoomRepository _gameRoomRepository;

    public GameRoomService(
        IGameRoomRepository gameRoomRepository
    )
    {
        _gameRoomRepository = gameRoomRepository;
    }

    public async Task<List<GameRoomDTO>> AggRoom(string roomId)
    {
        return await _gameRoomRepository.AggRoom1(roomId);
    }

    public async Task<GameRoomPO> GetOne(string roomId)
    {
        return await _gameRoomRepository.GetDbSet().Where(t => t.RoomId == roomId).FirstOrDefaultAsync();
    }

    public async Task SaveOne()
    {
        var p = new GameRoomPO();
        p.Platfrom = "DY";
        p.GameId = 1;
        p.RoomId = "1";
        p.AnchorId = "1";
        p.AvatarUrl = "haha";
        await _gameRoomRepository.UpsertAsync(p, new Dictionary<Expression<Func<GameRoomPO, object>>, string>
        {
            { t => t.AvatarUrl, "values(avatar_url)" }
        });

        //GameRoomPO p= GameRoomPO.Empty();
        //await _gameRoomRepository.InsertAsync(p,false);
    }
}