using Demo.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

        public async Task InsertAsync(T po, bool autoCommit = true)
        {
            await _dbSet.AddAsync(po);
            
            if (autoCommit)
            {
                await SaveChangesAsync();
            }
        }

        public async Task<int> DeleteByIdAsync(object id)
        {
            return await _dbSet.Where(t => EF.Property<object>(t, "Id").Equals(id)).ExecuteDeleteAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
