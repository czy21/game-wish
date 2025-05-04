using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    /// 心愿-项
    [Table("wish_item")]
    public class WishItemPO : BaseEntity<long,string>
    {
        [Column("user_id")]
        public long? UserId { get; set; }
        [Column("content")]
        public string? Content { get; set; }
        /// 截至时间
        [Column("limit_date")]
        public DateTime? LimitDate { get; set; }
    }
}
