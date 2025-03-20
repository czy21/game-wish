using Microsoft.EntityFrameworkCore;

namespace Demo.Repository;

public interface IRepositoryBase<T> where T : class
{

    DbSet<T> GetDbSet();

    Task InsertAsync(T po);

    Task<T?> SelectByIdAsync(object id);

    Task DeleteByIdAsync(object id);

}