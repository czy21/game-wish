namespace WishServer.Client.DY
{
    public class DYLiveDataTaskReq
    {
        public string roomid { get; set; }
        public string appid { get; set; }

        /*
         * 1. 评论：live_comment
         * 2. 礼物：live_gift
         * 3. 点赞：live_like"
         */
        public string msg_type { get; set; }
    }
}
