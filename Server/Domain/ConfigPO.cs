using Dapper.FluentMap.Mapping;
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
    public class ConfigPOMap : EntityMap<ConfigPO>
    {
        public ConfigPOMap()
        {
            Map(m => m.Id).ToColumn("id");
            Map(m => m.Key).ToColumn("key");
            Map(m => m.Value).ToColumn("value");
            Map(m => m.Remark).ToColumn("remark");
            Map(m => m.Category).ToColumn("category");
            Map(m => m.CreateTime).ToColumn("create_time");
            Map(m => m.CreateUser).ToColumn("create_user");
            Map(m => m.UpdateTime).ToColumn("update_time");
            Map(m => m.UpdateUser).ToColumn("update_user");
            Map(m => m.Deleted).ToColumn("deleted");
        }
    }
}
