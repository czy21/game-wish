using System.ComponentModel.DataAnnotations.Schema;
using Sunny.Framework.Core.Model;

namespace WishServer.Domain;

[Table("game_room")]
public class GameRoomPO : BaseEntity<long?, string>
{
    [Column("game_id")] public long? GameId { get; set; }

    /// 平台
    [Column("platfrom")]
    public string Platfrom { get; set; }

    /// 房间id
    [Column("room_id")]
    public string RoomId { get; set; }

    /// 主播id
    [Column("anchor_id")]
    public string AnchorId { get; set; }

    /// 头像
    [Column("avatar_url")]
    public string AvatarUrl { get; set; }

    /// 昵称
    [Column("nickname")]
    public string Nickname { get; set; }

    public static GameRoomPO Empty()
    {
        return new GameRoomPO
        {
            Id = default(long),
            GameId = default(long),
            Platfrom = string.Empty,
            RoomId = string.Empty,
            AnchorId = string.Empty,
            AvatarUrl = string.Empty,
            Nickname = string.Empty,
            CreateTime = default(DateTime),
            CreateUser = string.Empty,
            UpdateTime = default(DateTime),
            UpdateUser = string.Empty,
            Deleted = default
        };
    }
}