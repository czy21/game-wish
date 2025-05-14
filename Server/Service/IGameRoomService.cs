using WishServer.Domain;
using WishServer.Model.DTO;

namespace WishServer.Service;

public interface IGameRoomService : IServiceBase
{
    Task<List<GameRoomDTO>> AggRoom(string roomId);

    Task<GameRoomPO> GetOne(string roomId);

    Task SaveOne();
}