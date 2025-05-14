using Microsoft.EntityFrameworkCore;
using WishServer.Repository;

namespace WishServer.Service.impl;

public class ConfigService : ServiceBase, IConfigService
{
    private readonly IConfigRepository _configRepository;

    public ConfigService(IConfigRepository configRepository)
    {
        _configRepository = configRepository;
    }

    public async Task<T> GetValue<T>(string category, string key)
    {
        var po = await _configRepository.GetDbSet().Where(t => t.Category == category && t.Key == key).FirstOrDefaultAsync();
        return await Task.FromResult((T)Convert.ChangeType(po?.Value, typeof(T)));
    }

    public async Task<T> GetValue<T>(string category, string key, T defaultValue)
    {
        var po = await _configRepository.GetDbSet().Where(t => t.Category == category && t.Key == key).FirstOrDefaultAsync();
        return await Task.FromResult((T)Convert.ChangeType(po?.Value, typeof(T)) ?? defaultValue);
    }
}