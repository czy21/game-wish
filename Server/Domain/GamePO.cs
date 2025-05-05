using Dapper.FluentMap.Mapping;
using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    [Table("game")]
    public class GamePO : BaseEntity<long?, string?>
    {
        /// 编码
        [Column("code")]
        public string? Code { get; set; }
        /// 名称
        [Column("name")]
        public string? Name { get; set; }

        public static GamePO Empty()
        {
            return new ()
            {
                Id = default(long),
                Code = string.Empty,
                Name = string.Empty,
                CreateTime = default(DateTime),
                CreateUser = string.Empty,
                UpdateTime = default(DateTime),
                UpdateUser = string.Empty,
                Deleted = default(bool),
            };
        }
    }
    public class GamePOMap : EntityMap<GamePO>
    {
        public GamePOMap()
        {
            Map(m => m.Id).ToColumn("id");
            Map(m => m.Code).ToColumn("code");
            Map(m => m.Name).ToColumn("name");
            Map(m => m.CreateTime).ToColumn("create_time");
            Map(m => m.CreateUser).ToColumn("create_user");
            Map(m => m.UpdateTime).ToColumn("update_time");
            Map(m => m.UpdateUser).ToColumn("update_user");
            Map(m => m.Deleted).ToColumn("deleted");
        }
    }
}
