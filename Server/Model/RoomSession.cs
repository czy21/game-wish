namespace WishServer.Model
{
    public class RoomSession
    {
        public Session Session { get; set; }
        public List<RoomTask> Tasks { get; set; } = new List<RoomTask>()
        {
            new(){TaskType = "live_comment"},
            new(){TaskType = "live_gift"},
            new(){TaskType = "live_like"}
        };
    }
}
