namespace WishServer
{
    public class ConfigProperties
    {
        public string AppName { get; set; }
        public Platform Platform { get; set; } = new Platform();
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

    public class Platform
    {
        public string AppToken { get; set; }
        public PlatformDY DY { get; set; } = new PlatformDY();
        public PlatformKS KS { get; set; } = new PlatformKS();
    }


    public class PlatformDY
    {
        public PlatformOAuth OAuth { get; set; }
    }

    public class PlatformKS
    {
        public PlatformOAuth OAuth { get; set; }
    }

    public class PlatformOAuth
    {
        public string AppId { get; set; }

        public string AppSecret { get; set; }
    }


}