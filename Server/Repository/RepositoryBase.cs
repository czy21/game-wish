using Demo.Repository;
using Microsoft.EntityFrameworkCore;
using WishServer.Domain;

namespace WishServer.Repository
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected readonly DbMasterContext _context;
        protected readonly DbSet<T> _dbSet;

        public RepositoryBase(DbMasterContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public DbSet<T> GetDbSet()
        {
            return _dbSet;
        }

        public async Task<T?> SelectByIdAsync(object id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task InsertAsync(T po)
        {
            await _dbSet.AddAsync(po);
        }

        public async Task DeleteByIdAsync(object id)
        {
            await _dbSet.Where(t => EF.Property<object>(t, "Id").Equals(id)).ExecuteDeleteAsync();
        }
    }
}
