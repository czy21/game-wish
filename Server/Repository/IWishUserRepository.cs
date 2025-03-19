using WishServer.Domain;

namespace Demo.Repository;

public interface IWishUserRepository : IRepositoryBase
{
    Task<WishUserPO?> SelectById(long id);
}