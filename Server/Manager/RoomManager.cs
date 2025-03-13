using WishServer.Annotation;
using WishServer.Controllers;
using WishServer.Model;
using WishServer.Util;

namespace WishServer.Manager
{
    public class RoomManager : IMessageHandler
    {

        [OnMessage(MessageKind.ROOM_JOIN)]
        public async Task HandleRoomJoin(Session session, MessageDTO messageDTO,RoomJoinDTO roomJoinDTO)
        {
            List<Task> tasks = new();
            if (!session.AllRooms.TryGetValue(roomJoinDTO.ID, out var clientIds))
            {
                session.AllRooms[roomJoinDTO.ID] = [session.ClientId];
            }
            else
            {
                session.AllRooms[roomJoinDTO.ID].Add(session.ClientId);
            }
            tasks = WebSocketController.BroadMessage(session, (t) =>
            {
                return t == session.ClientId ? string.Format("加入 => {0}", roomJoinDTO.ID) : string.Format("{0}:{1} 加入 => {2}", session.ConnectionInfo.RemoteIpAddress, session.ConnectionInfo.RemotePort, roomJoinDTO.ID);
            });

            await Task.WhenAll(tasks);
        }

        [OnMessage(MessageKind.ROOM_CHAT)]
        public async Task HandleRoomChat(Session session, MessageDTO messageDTO,RoomChatDTO roomChatDTO)
        {
            List<Task> tasks = new();
            
            tasks = WebSocketController.BroadMessage(session, (t) =>
            {
                return t == session.ClientId ? string.Format("自己 => {0}", roomChatDTO.Content) : string.Format("{0}:{1} => {2}", session.ConnectionInfo.RemoteIpAddress, session.ConnectionInfo.RemotePort, roomChatDTO.Content);
            });
            await Task.WhenAll(tasks);
        }
    }
}
