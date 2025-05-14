namespace WishServer.Model.DY;

public class DYRoomSession : RoomSession
{
    public List<RoomTask> Tasks { get; set; } = new()
    {
        new RoomTask { TaskType = "live_comment" },
        new RoomTask { TaskType = "live_gift" },
        new RoomTask { TaskType = "live_like" }
    };
}