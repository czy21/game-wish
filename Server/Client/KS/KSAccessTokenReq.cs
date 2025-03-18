namespace WishServer.Client.KS
{
    public class KSAccessTokenReq
    {
        public string app_id { get; set; }
        public string app_secret { get; set; }
        public string grant_type { get; set; }
    }
}
