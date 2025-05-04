using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    [Table("game_round")]
    public class GameRoundPO : BaseEntity<long,string>
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
        /// 阵营
        [Column("camp")]
        public string? Camp { get; set; }
    }
}
