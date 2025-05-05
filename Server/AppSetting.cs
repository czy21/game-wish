namespace WishServer
{
    public class AppSetting
    {
        public string AppName { get; set; }
        public ConfigPlatform Platform { get; set; } = new ConfigPlatform();
        public ConfigData Data { get; set; } = new ConfigData();
    }

    public class ConfigData
    {
        public ConfigDataRedis Redis { get; set; } = new ConfigDataRedis();

        public ConfigDataMySQL MySQL { get; set; } = new ConfigDataMySQL();
    }

    public class ConfigDataRedis
    {
        public string Url { get; set; }
    }

    public class ConfigDataMySQL
    {
        public string Url { get; set; } 
    
    }

    public class ConfigPlatform
    {
        public string AppId { get; set; }
        public ConfigPlatformDY DY { get; set; } = new ConfigPlatformDY();
        public PlatformKS KS { get; set; } = new PlatformKS();
    }


    public class ConfigPlatformDY
    {
        public ConfigPlatformOAuth OAuth { get; set; }
    }

    public class PlatformKS
    {
        public ConfigPlatformOAuth OAuth { get; set; }
    }

    public class ConfigPlatformOAuth
    {
        public string AppId { get; set; }

        public string AppSecret { get; set; }
    }
}