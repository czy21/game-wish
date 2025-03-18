namespace WishServer.Client.KS
{
    public class KSAccessTokenRes
    {
        public int result { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }
}
