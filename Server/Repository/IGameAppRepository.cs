using Sunny.Framework.DB.Repository;
using WishServer.Domain;
using WishServer.Model.BO;

namespace WishServer.Repository
{
    public interface IGameAppRepository : IRepositoryBase<long?, GameAppPO>
    {
        Task<GameAppBO> SelectOneByPlatformAndGameCode(string platform,string gameCode);
    }
}
