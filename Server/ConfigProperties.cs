namespace WishServer
{
    public class ConfigProperties
    {
        public Platform Platform { get; set; } = new Platform();
        public ConfigData Data { get; set; } = new ConfigData();
    }


    public class ConfigData
    {
        public ConfigRedis Redis { get; set; } = new ConfigRedis();
    }

    public class ConfigRedis
    {
        public string Url { get; set; }
        public string Prefix { get; set; }
    }

    public class Platform
    {
        public PlatformDY DY { get; set; } = new PlatformDY();
    }

    public class PlatformDY
    {
        public string AppId { get; set; }

        public string AppSecret { get; set; }

        public string AppToken { get; set; }
    }


}