using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    [Table(name: "config")]
    public class ConfigPO : BaseEntity<long, string>
    {
        [Column(name: "key")] public string? Key { get; set; }

        [Column(name: "value")] public string? Value { get; set; }

        [Column(name: "category")] public string? Category { get; set; }

        [Column(name: "remark")] public string? Remark { get; set; }
    }
}
