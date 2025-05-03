using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    [Table(name: "game_user")]
    public class GameUserPO : BaseEntity<long, string>
    {
        [Column(name: "game_id")] public long? GameId { get; set; }
        [Column(name: "platform")] public string? Platform { get; set; }
        [Column(name: "user_id")] public string? UserId { get; set; }
        [Column(name: "avatar_url")] public string? AvatarUrl { get; set; }
        [Column(name: "nickname")] public string? Nickname { get; set; }
    }
}
