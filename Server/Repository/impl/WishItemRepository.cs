using Sunny.Framework.DB.Repository;
using WishServer.Domain;

namespace WishServer.Repository.impl;

public class WishItemRepository(DbMasterContext dbContext) : RepositoryBase<long, WishItemPO>(dbContext), IWishItemRepository
{
}