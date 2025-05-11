using AutoMapper;
using System.Text.Json.Nodes;
using WishServer.Domain;
using WishServer.Model.DTO;

namespace WishServer.AutoMapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<GameRoomPO, GameRoomDTO>();
            CreateMap<GameRoomDTO, GameRoomPO>();
        }
    }

    public class LiveMessageMapper
    {
        public static T MapFromDy<T>(JsonNode jsonObj) where T : LiveMessageBase, new()
        {
            return new T()
            {
                MsgId = jsonObj["msg_id"]?.ToString() ?? "",
                UserId = jsonObj["sec_openid"]?.ToString() ?? "",
                AvatarUrl = jsonObj["avatar_url"]?.ToString() ?? "",
                Nickname = jsonObj["nickname"]?.ToString() ?? "",
                Timestamp = long.Parse(jsonObj["timestamp"]?.ToString() ?? ""),
            };
        }
        public static LiveMessageComment MapFromDyComment(JsonNode jsonObj)
        {
            var t = MapFromDy<LiveMessageComment>(jsonObj);
            t.Content = jsonObj["content"]?.ToString() ?? "";
            return t;
        }

        public static LiveMessageLike MapFromDyLike(JsonNode jsonObj)
        {
            var msg = MapFromDy<LiveMessageLike>(jsonObj);
            msg.Num = long.Parse(jsonObj["num"]?.ToString() ?? "");
            return msg;
        }

        public static LiveMessageGift MapFromDyGift(JsonNode jsonObj)
        {
            var msg = MapFromDy<LiveMessageGift>(jsonObj);
            msg.GiftId = jsonObj["sec_gift_id"]?.ToString()??"";
            msg.GiftNum =long.Parse(jsonObj["gift_num"]?.ToString() ?? "");
            msg.GiftValue = long.Parse(jsonObj["gift_value"]?.ToString() ?? "");
            return msg;
        }
    }
}
