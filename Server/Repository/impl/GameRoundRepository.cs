using Sunny.Framework.DB.Repository;
using WishServer.Domain;

namespace WishServer.Repository.impl;

public class GameRoundRepository(AppDbContext dbContext) : RepositoryBase<long?, GameRoundPO>(dbContext), IGameRoundRepository
{
}