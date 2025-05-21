using Sunny.Framework.Cache;

namespace WishServer;

public class AppSetting
{
    /// <summary>
    ///     应用名称
    /// </summary>
    public string AppName { get; set; }

    public ConfigPlatform Platform { get; set; } = new();
    public ConfigData Data { get; set; } = new();
}

public class ConfigData
{
    public RedisProperties Redis { get; set; } = new();

    public ConfigDataMySQL MySQL { get; set; } = new();
}

public class ConfigDataMySQL
{
    public string Url { get; set; }
}

public class ConfigPlatform
{
    public string AppId { get; set; }
}