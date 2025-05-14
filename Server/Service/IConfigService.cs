namespace WishServer.Service;

public interface IConfigService : IServiceBase
{
    Task<T> GetValue<T>(string category, string key);

    Task<T> GetValue<T>(string category, string key, T defaultValue);
}