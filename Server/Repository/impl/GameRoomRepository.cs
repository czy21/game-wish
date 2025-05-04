using Sunny.Framework.DB.Repository;
using WishServer.Domain;

namespace WishServer.Repository.impl
{
    public class GameRoomRepository(AppDbContext dbContext) : RepositoryBase<long, GameRoomPO>(dbContext), IGameRoomRepository
    {
    }
}
