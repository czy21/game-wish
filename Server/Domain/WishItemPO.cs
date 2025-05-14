using System.ComponentModel.DataAnnotations.Schema;
using Sunny.Framework.Core.Model;

namespace WishServer.Domain;

/// 心愿-项
[Table("wish_item")]
public class WishItemPO : BaseEntity<long?, string>
{
    [Column("user_id")] public long? UserId { get; set; }

    [Column("content")] public string Content { get; set; }

    /// 截至时间
    [Column("limit_date")]
    public DateTime? LimitDate { get; set; }

    public static WishItemPO Empty()
    {
        return new WishItemPO
        {
            Id = default(long),
            UserId = default(long),
            Content = string.Empty,
            LimitDate = default(DateTime),
            CreateTime = default(DateTime),
            CreateUser = string.Empty,
            UpdateTime = default(DateTime),
            UpdateUser = string.Empty,
            Deleted = default
        };
    }
}