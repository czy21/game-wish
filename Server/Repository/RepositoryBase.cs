using Demo.Repository;

namespace WishServer.Repository
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected readonly DbMasterContext _dbMasterContext;

        public RepositoryBase(DbMasterContext dbMasterContext)
        {
            _dbMasterContext = dbMasterContext;
        }

        public async Task<T> InsertAsync(T po)
        {
            var re = await _dbMasterContext.Set<T>().AddAsync(po);
            return re.Entity;
        }

        public async Task<T?> SelectByIdAsync(long id)
        {
            return await _dbMasterContext.Set<T>().FindAsync(id);
        }
    }
}
