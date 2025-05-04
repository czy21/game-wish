using WishServer.Model.DTO;

namespace WishServer.Service
{
    public interface IGameRoomService : IServiceBase
    {
        Task<GameRoomDTO?> AggRoom(string roomId);
    }
}
