namespace WishServer.Client.DY
{
    public class DYAccessTokenReq
    {
        public string appid { get; set; }
        public string grant_type { get; set; }
        public string secret { get; set; }
    }
}
