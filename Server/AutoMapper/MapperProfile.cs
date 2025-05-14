using System.Text.Json.Nodes;
using AutoMapper;
using WishServer.Domain;
using WishServer.Model.DTO;

namespace WishServer.AutoMapper;

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
        return new T
        {
            MsgId = jsonObj["msg_id"]?.ToString(),
            UserId = jsonObj["sec_openid"]?.ToString(),
            AvatarUrl = jsonObj["avatar_url"]?.ToString(),
            Nickname = jsonObj["nickname"]?.ToString(),
            Timestamp = long.Parse(jsonObj["timestamp"]?.ToString() ?? "")
        };
    }

    public static LiveMessageCommentDTO MapFromDyGroup(JsonNode jsonObj, long? timestamp)
    {
        return new LiveMessageCommentDTO
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
        var msg = MapFromDy<LiveMessageCommentDTO>(jsonObj);
        msg.Content = jsonObj["content"]?.ToString() ?? "";
        return msg;
    }

    public static LiveMessageLikeDTO MapFromDyLike(JsonNode jsonObj)
    {
        var msg = MapFromDy<LiveMessageLikeDTO>(jsonObj);
        msg.Num = long.Parse(jsonObj["like_num"]?.ToString() ?? "");
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

    public static T MapFromKs<T>(JsonNode jsonObj, long timestamp) where T : LiveMessageDTOBase, new()
    {
        return new T
        {
            UserId = jsonObj["userInfo"]?["userId"]?.ToString(),
            AvatarUrl = jsonObj["userInfo"]?["headUrl"]?.ToString(),
            Nickname = jsonObj["userInfo"]?["userName"]?.ToString(),
            Timestamp = timestamp
        };
    }

    public static LiveMessageCommentDTO MapFromKsComment(JsonNode jsonObj, long timestamp)
    {
        var t = MapFromKs<LiveMessageCommentDTO>(jsonObj, timestamp);
        t.Content = jsonObj["content"]?.ToString() ?? "";
        return t;
    }

    public static LiveMessageLikeDTO MapFromKsLike(JsonNode jsonObj, long timestamp)
    {
        var msg = MapFromKs<LiveMessageLikeDTO>(jsonObj, timestamp);
        msg.Num = long.Parse(jsonObj["count"]?.ToString() ?? "");
        return msg;
    }

    public static LiveMessageGiftDTO MapFromKsGift(JsonNode jsonObj, long timestamp)
    {
        var msg = MapFromKs<LiveMessageGiftDTO>(jsonObj, timestamp);
        msg.GiftId = jsonObj["giftId"]?.ToString() ?? "";
        msg.GiftNum = long.Parse(jsonObj["giftCount"]?.ToString() ?? "");
        msg.GiftValue = long.Parse(jsonObj["giftTotalPrice"]?.ToString() ?? "") * 10;
        return msg;
    }
}