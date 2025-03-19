using Demo.Repository;
using WishServer.Domain;

namespace WishServer.Repository.impl;

public class WishUserRepository : RepositoryBase<WishUserPO>, IWishUserRepository
{
    public WishUserRepository(DbMasterContext dbContext) : base(dbContext)
    {

    }
}