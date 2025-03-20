using Microsoft.EntityFrameworkCore;

namespace Demo.Repository;

public interface IRepositoryBase<T> where T : class
{

    DbSet<T> GetDbSet();

    Task InsertAsync(T po, bool autoCommit = true);

    Task<T?> SelectByIdAsync(object id);

    Task<int> DeleteByIdAsync(object id);

    Task<int> SaveChangesAsync();
}