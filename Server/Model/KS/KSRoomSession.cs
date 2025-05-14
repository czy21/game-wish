namespace WishServer.Model.KS;

public class KSRoomSession : RoomSession
{
    public RoomTask Bind { get; set; } = new() { TaskType = "bind" };
}