using Sunny.Framework.Core.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain
{
    [Table(name: "game_gift")]
    public class GameGiftPO : BaseEntity<long, string>
    {
        [Column(name: "game_id")] public long? GameId { get; set; }
        [Column(name: "platform")] public string? Platform { get; set; }
        [Column(name: "room_id")] public string? RoomId { get; set; }
        [Column(name: "round_id")] public long? RoundId { get; set; }
        [Column(name: "user_id")] public string? UserId { get; set; }
        [Column(name: "gift_id")] public string? GiftId { get; set; }
        [Column(name: "gift_count")] public long? GiftCount { get; set; }
        [Column(name: "gift_money")] public long? GiftMoney { get; set; }
    }
}
