namespace Demo.Repository;

public interface IRepositoryBase<T> where T : class
{
    Task<T?> SelectByIdAsync(long id);

    Task<T> InsertAsync(T po);
}