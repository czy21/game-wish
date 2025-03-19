namespace WishServer.Model.DY
{
    public class DYLiveGiftMessage:DYMessageBase
    {
        public string sec_gift_id {  get; set; } // 加密的礼物id
        public long gift_num {  get; set; }  // 送出的礼物数量
        public long gift_value {  get; set; } // 礼物总价值，单位分
    }
}
