using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    /// 心愿-用户
    [Table("wish_user")]
    public class WishUserPO : BaseEntity<long,string>
    {
        /// 平台;DY;KS
        [Column("platform")]
        public string? Platform { get; set; }
        /// 房间id
        [Column("room_id")]
        public string? RoomId { get; set; }
        /// 主播-uid
        [Column("anchor_uid")]
        public string? AnchorUid { get; set; }
        /// 观众-uid
        [Column("audience_uid")]
        public string? AudienceUid { get; set; }
    }
}
