using Sunny.Framework.DB.Repository;
using WishServer.Domain;

namespace WishServer.Repository
{
    public interface IWishItemRepository : IRepositoryBase<long?, WishItemPO>
    {

    }
}
