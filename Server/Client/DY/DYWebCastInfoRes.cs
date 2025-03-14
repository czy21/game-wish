namespace WishServer.Client.DY
{
    public class DYWebCastInfoRes
    {
        public int? errcode { get; set; }
        public string? errmsg { get; set; }
        public DYWebCastInfoData? data { get; set; }
    }

    public class DYWebCastInfoData
    {
        public DYWebCastInfo? info { get; set; }
    }

    public class DYWebCastInfo
    {
        public long? room_id { get; set; }
        public string? anchor_open_id { get; set; }

        public string? avatar_url { get; set; }

        public string? nick_name { get; set; }
    }
}
