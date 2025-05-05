using Dapper.FluentMap.Mapping;
using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    [Table("game_room")]
    public class GameRoomPO : BaseEntity<long?, string?>
    {
        [Column("game_id")]
        public long? GameId { get; set; }
        /// 平台
        [Column("platfrom")]
        public string? Platfrom { get; set; }
        /// 房间id
        [Column("room_id")]
        public string? RoomId { get; set; }
        /// 主播id
        [Column("anchor_id")]
        public string? AnchorId { get; set; }
        /// 头像
        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }
        /// 昵称
        [Column("nickname")]
        public string? Nickname { get; set; }

        public static GameRoomPO Empty()
        {
            return new ()
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
                Deleted = default(bool),
            };
        }
    }
    public class GameRoomPOMap : EntityMap<GameRoomPO>
    {
        public GameRoomPOMap()
        {
            Map(m => m.Id).ToColumn("id");
            Map(m => m.GameId).ToColumn("game_id");
            Map(m => m.Platfrom).ToColumn("platfrom");
            Map(m => m.RoomId).ToColumn("room_id");
            Map(m => m.AnchorId).ToColumn("anchor_id");
            Map(m => m.AvatarUrl).ToColumn("avatar_url");
            Map(m => m.Nickname).ToColumn("nickname");
            Map(m => m.CreateTime).ToColumn("create_time");
            Map(m => m.CreateUser).ToColumn("create_user");
            Map(m => m.UpdateTime).ToColumn("update_time");
            Map(m => m.UpdateUser).ToColumn("update_user");
            Map(m => m.Deleted).ToColumn("deleted");
        }
    }
}
