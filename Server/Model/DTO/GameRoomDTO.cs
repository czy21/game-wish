using WishServer.Domain;

namespace WishServer.Model.DTO
{
    public class GameRoomDTO : GameRoomPO
    {
        public long? RoundCount { get; set; }

        public List<GameRoundPO> Rounds { get; set; }
    }
}
