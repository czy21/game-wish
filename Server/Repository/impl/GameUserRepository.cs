using Sunny.Framework.DB.Repository;
using WishServer.Domain;

namespace WishServer.Repository.impl;

public class GameUserRepository(AppDbContext dbContext) : RepositoryBase<long?, GameUserPO>(dbContext), IGameUserRepository
{
}