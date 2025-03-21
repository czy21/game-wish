using Microsoft.EntityFrameworkCore;

namespace WishServer.Repository;

public interface IRepositoryBase<K, T> where T : class
{
    DbSet<T> GetDbSet();

    Task<int> InsertAsync(T po, bool ignoreNull = true, bool autoCommit = true);

    Task<int> BatchInsertAsync(List<T> pos);

    Task<int> UpdateAsync(T po, bool ignoreNull = true, bool autoCommit = true);

    Task<T?> SelectByIdAsync(K id);

    Task<int> DeleteByIdAsync(K id);

    Task<int> SaveChangesAsync();
}