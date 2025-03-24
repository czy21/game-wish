using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain;

[Table(name: "wish_item")]
public class WishItemPO : BaseEntity<long, string>
{
    [Column(name: "user_id")] public long? UserId { get; set; }

    [Column(name: "content")] public string? Content { get; set; }

    [Column(name: "limitDate")] public DateTime? LimitDate { get; set; }

}