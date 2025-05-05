using Dapper.FluentMap.Mapping;
using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    [Table("game_gift")]
    public class GameGiftPO : BaseEntity<long?, string?>
    {
        [Column("game_id")]
        public long? GameId { get; set; }
        /// 平台
        [Column("platform")]
        public string? Platform { get; set; }
        /// 房间id
        [Column("room_id")]
        public string? RoomId { get; set; }
        /// 回合id
        [Column("round_id")]
        public long? RoundId { get; set; }
        /// 用户id
        [Column("user_id")]
        public string? UserId { get; set; }
        /// 礼物id
        [Column("gift_id")]
        public string? GiftId { get; set; }
        /// 礼物数量
        [Column("gift_count")]
        public long? GiftCount { get; set; }
        /// 礼物价值;人民币(分)
        [Column("gift_money")]
        public long? GiftMoney { get; set; }

        public static GameGiftPO Empty()
        {
            return new ()
            {
                Id = default(long),
                GameId = default(long),
                Platform = string.Empty,
                RoomId = string.Empty,
                RoundId = default(long),
                UserId = string.Empty,
                GiftId = string.Empty,
                GiftCount = default(long),
                GiftMoney = default(long),
                CreateTime = default(DateTime),
                CreateUser = string.Empty,
                UpdateTime = default(DateTime),
                UpdateUser = string.Empty,
                Deleted = default(bool),
            };
        }
    }
    public class GameGiftPOMap : EntityMap<GameGiftPO>
    {
        public GameGiftPOMap()
        {
            Map(m => m.Id).ToColumn("id");
            Map(m => m.GameId).ToColumn("game_id");
            Map(m => m.Platform).ToColumn("platform");
            Map(m => m.RoomId).ToColumn("room_id");
            Map(m => m.RoundId).ToColumn("round_id");
            Map(m => m.UserId).ToColumn("user_id");
            Map(m => m.GiftId).ToColumn("gift_id");
            Map(m => m.GiftCount).ToColumn("gift_count");
            Map(m => m.GiftMoney).ToColumn("gift_money");
            Map(m => m.CreateTime).ToColumn("create_time");
            Map(m => m.CreateUser).ToColumn("create_user");
            Map(m => m.UpdateTime).ToColumn("update_time");
            Map(m => m.UpdateUser).ToColumn("update_user");
            Map(m => m.Deleted).ToColumn("deleted");
        }
    }
}
