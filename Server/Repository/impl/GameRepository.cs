using Sunny.Framework.DB.Repository;
using WishServer.Domain;

namespace WishServer.Repository.impl
{
    public class GameRepository(AppDbContext dbContext) : RepositoryBase<long?, GamePO>(dbContext), IGameRepository
    {
    }
}
