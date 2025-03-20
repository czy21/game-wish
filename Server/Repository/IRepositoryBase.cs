using Microsoft.EntityFrameworkCore;

namespace WishServer.Repository;

public interface IRepositoryBase<T> where T : class
{

    DbSet<T> GetDbSet();

    Task InsertAsync(T po, bool ignoreNull = true, bool autoCommit = true);

    Task UpdateAsync(T po);

    Task UpdateByIdAsync(object id, T po);

    Task<T?> SelectByIdAsync(object id);

    Task<int> DeleteByIdAsync(object id);

    Task<int> SaveChangesAsync();
}