using AutoMapper;
using Google.Protobuf.WellKnownTypes;
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
        public static T MapFromDy<T>(JsonNode jsonObj) where T : LiveMessageDTOBase, new()
        {
            return new T()
            {
                MsgId = jsonObj["msg_id"]?.ToString(),
                UserId = jsonObj["sec_openid"]?.ToString(),
                AvatarUrl = jsonObj["avatar_url"]?.ToString(),
                Nickname = jsonObj["nickname"]?.ToString(),
                Timestamp = long.Parse(jsonObj["timestamp"]?.ToString() ?? ""),
            };
        }

        public static LiveMessageCommentDTO MapFromDyGroup(JsonNode jsonObj, long? timestamp)
        {
            return new LiveMessageCommentDTO()
            {
                UserId = jsonObj["open_id"]?.ToString(),
                AvatarUrl = jsonObj["avatar_url"]?.ToString(),
                Nickname = jsonObj["nickname"]?.ToString(),
                Content = jsonObj["group_id"]?.ToString(),
                Timestamp = timestamp
            };
        }

        public static LiveMessageCommentDTO MapFromDyComment(JsonNode jsonObj)
        {
            var t = MapFromDy<LiveMessageCommentDTO>(jsonObj);
            t.Content = jsonObj["content"]?.ToString() ?? "";
            return t;
        }

        public static LiveMessageLikeDTO MapFromDyLike(JsonNode jsonObj)
        {
            var msg = MapFromDy<LiveMessageLikeDTO>(jsonObj);
            msg.Num = long.Parse(jsonObj["num"]?.ToString() ?? "");
            return msg;
        }

        public static LiveMessageGiftDTO MapFromDyGift(JsonNode jsonObj)
        {
            var msg = MapFromDy<LiveMessageGiftDTO>(jsonObj);
            msg.GiftId = jsonObj["sec_gift_id"]?.ToString() ?? "";
            msg.GiftNum = long.Parse(jsonObj["gift_num"]?.ToString() ?? "");
            msg.GiftValue = long.Parse(jsonObj["gift_value"]?.ToString() ?? "");
            return msg;
        }
    }
}
