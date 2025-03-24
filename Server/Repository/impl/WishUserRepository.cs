using Sunny.Framework.DB.Repository;
using WishServer.Domain;

namespace WishServer.Repository.impl;

public class WishUserRepository(DbMasterContext dbContext) : RepositoryBase<long, WishUserPO>(dbContext), IWishUserRepository
{
}