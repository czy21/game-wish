using Dapper.FluentMap.Mapping;
using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    /// 心愿-用户
    [Table("wish_user")]
    public class WishUserPO : BaseEntity<long?, string?>
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

        public static WishUserPO Empty()
        {
            return new ()
            {
                Id = default(long),
                Platform = string.Empty,
                RoomId = string.Empty,
                AnchorUid = string.Empty,
                AudienceUid = string.Empty,
                CreateTime = default(DateTime),
                CreateUser = string.Empty,
                UpdateTime = default(DateTime),
                UpdateUser = string.Empty,
                Deleted = default(bool),
            };
        }
    }
    public class WishUserPOMap : EntityMap<WishUserPO>
    {
        public WishUserPOMap()
        {
            Map(m => m.Id).ToColumn("id");
            Map(m => m.Platform).ToColumn("platform");
            Map(m => m.RoomId).ToColumn("room_id");
            Map(m => m.AnchorUid).ToColumn("anchor_uid");
            Map(m => m.AudienceUid).ToColumn("audience_uid");
            Map(m => m.CreateTime).ToColumn("create_time");
            Map(m => m.CreateUser).ToColumn("create_user");
            Map(m => m.UpdateTime).ToColumn("update_time");
            Map(m => m.UpdateUser).ToColumn("update_user");
            Map(m => m.Deleted).ToColumn("deleted");
        }
    }
}
