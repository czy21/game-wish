using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    [Table("game")]
    public class GamePO : BaseEntity<long,string>
    {
        /// 编码
        [Column("code")]
        public string? Code { get; set; }
        /// 名称
        [Column("name")]
        public string? Name { get; set; }
    }
}
