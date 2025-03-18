namespace WishServer.Client.DY
{
    public class DYLiveDataAckReq
    {
        public string room_id {  get; set; }
        public string app_id {  get; set; }
        public int ack_type {  get; set; } //上报类型，1：收到推送；2：完成处理
        public string data { get; set; }
    }

    public class DYLiveDataAckReqData
    {
        public string msg_id {  get; set; }
        public string msg_type { get; set;}
        public int client_time {  get; set; }
    }
}
