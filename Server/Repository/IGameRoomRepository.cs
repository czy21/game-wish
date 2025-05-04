using Sunny.Framework.DB.Repository;
using WishServer.Domain;
using WishServer.Model.DTO;

namespace WishServer.Repository
{
    public interface IGameRoomRepository : IRepositoryBase<long?, GameRoomPO>
    {
        Task<GameRoomDTO?> AggRoom(string roomId);
    }
}
