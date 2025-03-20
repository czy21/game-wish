using Microsoft.EntityFrameworkCore;

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

        public async Task InsertAsync(T po, bool ignoreNull = true, bool autoCommit = true)
        {
            if (ignoreNull)
            {
                var entry = _context.Entry(po);

                var props = entry.Properties.Where(p => p.CurrentValue != null && p.Metadata.GetColumnName() != "id").ToList();

                string columns = string.Join(",", props.Select(p => p.Metadata.GetColumnName()));

                string values = string.Join(",", Enumerable.Range(0, props.Count).Select(t => $"@p{t}").ToList());

                string sql = $"INSERT INTO {entry.Metadata.GetTableName()} ({columns}) VALUES ({values})";

                object[]? parameters = props.Select(p => p.CurrentValue ?? DBNull.Value).ToArray();

                await _context.Database.ExecuteSqlRawAsync(sql, parameters);
            }
            else
            {
                await _dbSet.AddAsync(po);
                if (autoCommit)
                {
                    await SaveChangesAsync();
                }
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
