using Sunny.Framework.DB.Repository;
using WishServer.Domain;

namespace WishServer.Repository.impl;

public class WishUserRepository : RepositoryBase<long, WishUserPO>, IWishUserRepository
{
    public WishUserRepository(DbMasterContext dbContext) : base(dbContext)
    {

    }
}