using System.ComponentModel.DataAnnotations.Schema;
using Sunny.Framework.Core.Model;

namespace WishServer.Domain;

[Table("game_user")]
public class GameUserPO : BaseEntity<long?, string>
{
    [Column("game_id")] public long? GameId { get; set; }

    /// 平台
    [Column("platform")]
    public string Platform { get; set; }

    /// 用户id
    [Column("user_id")]
    public string UserId { get; set; }

    /// 头像地址
    [Column("avatar_url")]
    public string AvatarUrl { get; set; }

    /// 昵称
    [Column("nickname")]
    public string Nickname { get; set; }

    public static GameUserPO Empty()
    {
        return new GameUserPO
        {
            Id = default(long),
            GameId = default(long),
            Platform = string.Empty,
            UserId = string.Empty,
            AvatarUrl = string.Empty,
            Nickname = string.Empty,
            CreateTime = default(DateTime),
            CreateUser = string.Empty,
            UpdateTime = default(DateTime),
            UpdateUser = string.Empty,
            Deleted = default
        };
    }
}