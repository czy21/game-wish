namespace WishServer.Model
{
    public class DYRoomSession : RoomSession
    {
        public List<RoomTask> Tasks { get; set; } = new()
        {
            new(){TaskType = "live_comment"},
            new(){TaskType = "live_gift"},
            new(){TaskType = "live_like"}
        };
    }
}
