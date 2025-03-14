namespace WishServer.Client.DY
{
    public class DYAccessTokenRes
    {
        public int? err_no {  get; set; }
        public string? err_tips {  get; set; }
        public DYAccessTokenData? data { get; set; }
    }

    public class DYAccessTokenData
    {
        public string? access_token {  get; set; }
        public int expires_in {  get; set; }
    }
}
