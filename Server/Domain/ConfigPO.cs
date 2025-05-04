using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    [Table("config")]
    public class ConfigPO : BaseEntity<long?, string?>
    {
        [Column("key")]
        public string? Key { get; set; }
        [Column("value")]
        public string? Value { get; set; }
        [Column("remark")]
        public string? Remark { get; set; }
        [Column("category")]
        public string? Category { get; set; }

        public static ConfigPO Empty()
        {
            return new ()
            {
                Id = default(long),
                Key = string.Empty,
                Value = string.Empty,
                Remark = string.Empty,
                Category = string.Empty,
                CreateTime = default(DateTime),
                CreateUser = string.Empty,
                UpdateTime = default(DateTime),
                UpdateUser = string.Empty,
                Deleted = default(bool),
            };
        }
    }
}
