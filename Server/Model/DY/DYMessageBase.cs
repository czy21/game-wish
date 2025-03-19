namespace WishServer.Model.DY
{
    public class DYMessageBase
    {
        // https://bytedance.larkoffice.com/wiki/wikcnQe5jesCAbyUzsGx8xQeBNh
        public string msg_id {  get; set; }
        public string sec_openid {  get; set; } // 用户的加密openid，当前其实没有加密
        public string avatar_url {  get; set; } // 用户头像
        public string nickname { get; set; } // 用户的加密openid，当前其实没有加密
        public long timestamp {  get; set; }
    }
}
