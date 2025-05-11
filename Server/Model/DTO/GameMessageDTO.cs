namespace WishServer.Model.DTO
{

    public class GameMessageDTO<T>
    {
        public string MsgId { get; set; }
        public string MsgType { get; set; }
        public List<T> Msgs { get; set; }

        public PlatformEnum Platform { get; set; }
        public string RoomId { get; set; }
        public string AnchorUrl { get; set; }
        public string Nickname { get; set; }
    }

    public class LiveMessageDTOBase
    {
        public string MsgId { get; set; }     //  消息Id
        public string UserId { get; set; }    // 用户Id
        public string AvatarUrl { get; set; } // 用户头像
        public string Nickname { get; set; }  // 用户昵称
        public long Timestamp { get; set; }
    }

    public class LiveMessageCommentDTO : LiveMessageDTOBase
    {
        public string Content;
    }

    public class LiveMessageGiftDTO : LiveMessageDTOBase
    {
        public string GiftId { get; set; }
        public string GiftName { get; set; }
        public long GiftNum { get; set; }   // 礼物数量
        public long GiftValue { get; set; } // 礼物总价值，单位分
    }

    public class LiveMessageLikeDTO : LiveMessageDTOBase
    {
        public long Num; // 点赞数量
    }
}
