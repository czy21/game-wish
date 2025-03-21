using Microsoft.EntityFrameworkCore;

namespace WishServer.Repository
{
    public class RepositoryBase<K, T> : IRepositoryBase<K, T> where T : class
    {
        protected readonly DbMasterContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public RepositoryBase(DbMasterContext context)
        {
            _dbContext = context;
            _dbSet = context.Set<T>();
        }

        public DbSet<T> GetDbSet()
        {
            return _dbSet;
        }

        public async Task<int> InsertAsync(T po, bool ignoreNull = true, bool autoCommit = true)
        {
            if (ignoreNull)
            {
                var entry = _dbContext.Entry(po);

                string idColumnName = entry.Property("Id").Metadata.GetColumnName();

                var props = entry.Properties.Where(p => p.CurrentValue != null && p.Metadata.GetColumnName() != idColumnName).ToList();

                string columns = string.Join(",", props.Select(p => p.Metadata.GetColumnName()));

                string values = string.Join(",", Enumerable.Range(0, props.Count).Select(t => $"@p{t}").ToList());

                string sql = $"INSERT INTO {entry.Metadata.GetTableName()} ({columns}) VALUES ({values});SELECT LAST_INSERT_ID() as {idColumnName};";

                object[] parameters = props.Select(p => p.CurrentValue ?? DBNull.Value).ToArray();

                var result = await _dbContext.Database.SqlQueryRaw<K>(sql, parameters).ToListAsync();
                entry.Property("Id").CurrentValue = result.FirstOrDefault();
                return await Task.FromResult(result.Count).ConfigureAwait(false);
            }
            else
            {
                await _dbSet.AddAsync(po);
                if (autoCommit) return await SaveChangesAsync();
            }
            return await Task.FromResult(0).ConfigureAwait(false);
        }

        public async Task<int> BatchInsertAsync(List<T> pos)
        {
            if (pos == null || pos.Count == 0) return await Task.FromResult(0).ConfigureAwait(false);

            var entityType = _dbContext.Model.FindEntityType(typeof(T));

            var tableName = entityType!.GetTableName();

            var properties = entityType!.GetProperties().Where(t => t.PropertyInfo?.Name != "Id");

            int i = 0;

            var vars = new List<string>();
            var vals = new List<object?>();

            foreach (var t in pos)
            {
                vars.Add($"({string.Join(", ", properties.Select(p => $"@p{i++}"))})");
                vals.AddRange(properties.Select(p => p.PropertyInfo?.GetValue(t)));
            }

            var sql = $"INSERT INTO {tableName} ({string.Join(",", properties.Select(p => p.GetColumnName()))}) VALUES {string.Join(",", vars)}";

            return await _dbContext.Database.ExecuteSqlRawAsync(sql, [.. vals]);
        }

        public async Task<int> UpdateAsync(T po, bool ignoreNull = true, bool autoCommit = true)
        {
            if (ignoreNull)
            {
                var entry = _dbContext.Entry(po);

                object? idValue = entry.Property("Id").CurrentValue;

                if (idValue == null) return await Task.FromResult(0).ConfigureAwait(false);

                string idColumnName = entry.Property("Id").Metadata.GetColumnName();

                var props = entry.Properties.Where(p => p.CurrentValue != null && p.Metadata.GetColumnName() != idColumnName).ToList();

                string values = string.Join(",", Enumerable.Range(0, props.Count).Select(t => $"set {props[t].Metadata.GetColumnName()}=@p{t + 1}").ToList());

                string sql = $"update {entry.Metadata.GetTableName()} {values} where {idColumnName} = @p0";

                object[] parameters = props.Select(p => p.CurrentValue ?? DBNull.Value).ToArray();

                return await _dbContext.Database.ExecuteSqlRawAsync(sql, [idValue, .. parameters]);
            }
            else
            {
                _dbSet.Update(po);
                if (autoCommit) return await SaveChangesAsync();
            }
            return await Task.FromResult(0).ConfigureAwait(false);
        }

        public async Task<T?> SelectByIdAsync(K id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<int> DeleteByIdAsync(K id)
        {
            return await _dbSet.Where(t => EF.Property<K>(t, "Id").Equals(id)).ExecuteDeleteAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}