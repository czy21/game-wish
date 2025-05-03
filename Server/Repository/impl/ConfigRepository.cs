using Sunny.Framework.DB.Repository;
using WishServer.Domain;

namespace WishServer.Repository.impl
{
    public class ConfigRepository(AppDbContext dbContext) : RepositoryBase<long, ConfigPO>(dbContext), IConfigRepository
    {
    }
}
