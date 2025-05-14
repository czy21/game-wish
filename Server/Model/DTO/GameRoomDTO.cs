using WishServer.Domain;

namespace WishServer.Model.DTO;

public class GameRoomDTO : GameRoomPO
{
    public GamePO Game { get; set; }
    public GameAppPO GameApp { get; set; }
    public long? RoundCount { get; set; }
    public List<GameRoundPO> Rounds { get; set; }
}