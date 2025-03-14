using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
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
            HashSet<string> clientIds = WebSocketController.ROOM_DICT.AddOrUpdate(roomMessage.ID,
                k => [session.ClientId],
                (k, v) =>
                {
                    v.Add(session.ClientId);
                    return v;
                });
            tasks = WebSocketController.BroadMessage(session, clientIds, (t) =>
            {
                RoomMessage dto = new()
                {
                    Kind = messageDTO.Kind,
                    ID = roomMessage.ID,
                    Count = clientIds.Count,
                    Content = t == session.ClientId ? $"加入房间 => {roomMessage.ID}" : $"客户端: {session.ConnectionInfo.RemoteIpAddress}:{session.ConnectionInfo.RemotePort} 加入房间 => {roomMessage.ID}"
                };
                return dto;
            });

            await Task.WhenAll(tasks);
        }

        [OnMessage(MessageKind.ROOM_CHAT)]
        public async Task HandleRoomChat(Session session, MessageDTO messageDTO, RoomMessage roomMessage)
        {
            List<Task> tasks = [];

            if (WebSocketController.ROOM_DICT.TryGetValue(roomMessage.ID, out var clientIds) && clientIds.Contains(session.ClientId))
            {
                tasks = WebSocketController.BroadMessage(session, clientIds, (t) =>
                {
                    RoomMessage dto = new()
                    {
                        Kind = messageDTO.Kind,
                        ID = roomMessage.ID,
                        Count = clientIds.Count,
                        Content = t == session.ClientId ? $"自己 => {roomMessage.Content}" : $"客户端 {session.ConnectionInfo.RemoteIpAddress}:{session.ConnectionInfo.RemotePort} => {roomMessage.Content}"

                    };
                    return dto;
                });
            }

            await Task.WhenAll(tasks);
        }
    }
}
