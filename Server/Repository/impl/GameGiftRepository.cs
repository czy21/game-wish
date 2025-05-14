using Sunny.Framework.DB.Repository;
using WishServer.Domain;

namespace WishServer.Repository.impl;

public class GameGiftRepository(AppDbContext dbContext) : RepositoryBase<long?, GameGiftPO>(dbContext), IGameGiftRepository
{
}