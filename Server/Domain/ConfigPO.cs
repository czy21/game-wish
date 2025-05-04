using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    [Table("config")]
    public class ConfigPO : BaseEntity<long,string>
    {
        [Column("key")]
        public string? Key { get; set; }
        [Column("value")]
        public string? Value { get; set; }
        [Column("remark")]
        public string? Remark { get; set; }
        [Column("category")]
        public string? Category { get; set; }
    }
}
