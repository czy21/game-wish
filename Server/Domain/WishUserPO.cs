using System.ComponentModel.DataAnnotations.Schema;

namespace WishServer.Domain;

[Table(name: "wish_user")]
public class WishUserPO : BaseEntity<long, string>
{
    [Column(name: "platform")] public string? Platform { get; set; }

    [Column(name: "room_id")] public string? RoomId { get; set; }

    [Column(name: "anchor_uid")] public string? AnchorUid { get; set; }

    [Column(name: "audience_uid")] public string? AudienceUid { get; set; }

}