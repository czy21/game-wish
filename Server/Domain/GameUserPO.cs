using Dapper.FluentMap.Mapping;
using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    [Table("game_user")]
    public class GameUserPO : BaseEntity<long?, string?>
    {
        [Column("game_id")]
        public long? GameId { get; set; }
        /// 平台
        [Column("platform")]
        public string? Platform { get; set; }
        /// 用户id
        [Column("user_id")]
        public string? UserId { get; set; }
        /// 头像地址
        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }
        /// 昵称
        [Column("nickname")]
        public string? Nickname { get; set; }

        public static GameUserPO Empty()
        {
            return new ()
            {
                Id = default(long),
                GameId = default(long),
                Platform = string.Empty,
                UserId = string.Empty,
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
    public class GameUserPOMap : EntityMap<GameUserPO>
    {
        public GameUserPOMap()
        {
            Map(m => m.Id).ToColumn("id");
            Map(m => m.GameId).ToColumn("game_id");
            Map(m => m.Platform).ToColumn("platform");
            Map(m => m.UserId).ToColumn("user_id");
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
