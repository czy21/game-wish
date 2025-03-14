namespace WishServer.Model
{
    /*
     * 1-2: 模块编码
     * 3-4: 动作编码
     */
    public enum MessageKind
    {
        ROOM_CREATE = 1001,
        ROOM_JOIN = 1002,
        ROOM_LEAVE = 1003,
        ROOM_CHAT = 1004
    }
}
