using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    public class BaseEntity<TK, TU>
    {
        [Column(name: "id")] public TK? Id { get; set; }
        [Column(name: "create_time")] public DateTime? CreateTime { get; set; }
        [Column(name: "update_time")] public DateTime? UpdateTime { get; set; }
        [Column(name: "create_user")] public TU? CreateUser { get; set; }
        [Column(name: "update_user")] public TU? UpdateUser { get; set; }
    }

    public abstract class BaseEntity : BaseEntity<string, string>
    {
    }
}