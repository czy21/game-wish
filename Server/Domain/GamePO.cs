using System.ComponentModel.DataAnnotations.Schema;
using Sunny.Framework.Core.Model;

namespace WishServer.Domain;

[Table("game")]
public class GamePO : BaseEntity<long?, string>
{
    /// 编码
    [Column("code")]
    public string Code { get; set; }

    /// 名称
    [Column("name")]
    public string Name { get; set; }

    public static GamePO Empty()
    {
        return new GamePO
        {
            Id = default(long),
            Code = string.Empty,
            Name = string.Empty,
            CreateTime = default(DateTime),
            CreateUser = string.Empty,
            UpdateTime = default(DateTime),
            UpdateUser = string.Empty,
            Deleted = default
        };
    }
}