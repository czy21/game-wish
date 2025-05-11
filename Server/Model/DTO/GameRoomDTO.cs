using WishServer.Domain;

namespace WishServer.Model.DTO
{
    public class GameRoomDTO: GameRoomPO
    {
        public GamePO Game { get; set; }
    }
}