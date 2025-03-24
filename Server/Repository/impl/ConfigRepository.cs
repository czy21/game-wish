using Sunny.Framework.DB.Repository;
using WishServer.Domain;

namespace WishServer.Repository.impl
{
    public class ConfigRepository(DbMasterContext dbContext) : RepositoryBase<long, ConfigPO>(dbContext), IConfigRepository
    {
    }
}
