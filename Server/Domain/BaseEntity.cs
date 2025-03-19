using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    public class BaseEntity<TId, TUser>
    {
        [Column(name: "id")] public TId? Id { get; set; }
        [Column(name: "create_time")] public DateTime CreateTime { get; set; }
        [Column(name: "update_time")] public DateTime UpdateTime { get; set; }
        [Column(name: "create_user")] public TUser? CreateUser { get; set; }
        [Column(name: "update_user")] public TUser? UpdateUser { get; set; }
    }

    public abstract class BaseEntity : BaseEntity<string, string>
    {
    }
}