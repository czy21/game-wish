using WishServer.Annotation;
using WishServer.Controllers;
using WishServer.Model;

namespace WishServer.Manager
{
    public class RoomManager : IMessageHandler
    {

        [OnMessage(MessageKind.ROOM_JOIN)]
        public async Task HandleRoomJoin(Session session, MessageDTO messageDTO, RoomMessage roomMessage)
        {
            List<Task> tasks = [];
            if (!session.AllRooms.TryGetValue(roomMessage.ID, out var clientIds))
            {
                session.AllRooms[roomMessage.ID] = [session.ClientId];
            }
            else
            {
                session.AllRooms[roomMessage.ID].Add(session.ClientId);
            }
            tasks = WebSocketController.BroadMessage(session, (t) =>
            {
                RoomMessage dto = new()
                {
                    Kind = messageDTO.Kind,
                    ID = roomMessage.ID,
                    Content = t == session.ClientId ? string.Format("加入 => {0}", roomMessage.ID) : string.Format("{0}:{1} 加入 => {2}", session.ConnectionInfo.RemoteIpAddress, session.ConnectionInfo.RemotePort, roomMessage.ID)
                };
                return dto;
            });

            await Task.WhenAll(tasks);
        }

        [OnMessage(MessageKind.ROOM_CHAT)]
        public async Task HandleRoomChat(Session session, MessageDTO messageDTO, RoomMessage roomMessage)
        {
            List<Task> tasks = [];

            tasks = WebSocketController.BroadMessage(session, (t) =>
            {
                RoomMessage dto = new()
                {
                    Kind = messageDTO.Kind,
                    ID = roomMessage.ID,
                    Content = t == session.ClientId ? string.Format("自己 => {0}", roomMessage.Content) : string.Format("{0}:{1} => {2}", session.ConnectionInfo.RemoteIpAddress, session.ConnectionInfo.RemotePort, roomMessage.Content)
                };
                return dto;
            });
            await Task.WhenAll(tasks);
        }
    }
}
