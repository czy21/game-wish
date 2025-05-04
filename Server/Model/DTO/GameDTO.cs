using WishServer.Domain;

namespace WishServer.Model.DTO
{
    public class GameRoomDTO : GameRoomPO
    {
        public long? RoundCount { get; set; }
    }
}
