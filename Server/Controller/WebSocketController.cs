using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using WishServer.Annotation;
using WishServer.Manager;
using WishServer.Model;
using WishServer.Util;

namespace WishServer.Controllers
{
    public class WebSocketController : ControllerBase
    {

        private readonly ILogger<UserController> _logger;
        private readonly IEnumerable<IMessageHandler> _messageHandlers;

        public WebSocketController(ILogger<UserController> logger, IEnumerable<IMessageHandler> messageHandlers)
        {
            _logger = logger;
            _messageHandlers = messageHandlers;
        }

        private static readonly ConcurrentDictionary<string, Session> _clients = new();
        private static readonly ConcurrentDictionary<string, List<string>> _rooms = new();

        [Route("/ws")]
        public async Task Get()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            string clientId = Guid.NewGuid().ToString();
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            Session session = new()
            {
                ClientId = clientId,
                ConnectionInfo = HttpContext.Connection,
                WebSocket = webSocket,
                AllClients = _clients,
                AllRooms = _rooms
            };

            _clients.TryAdd(clientId, session);

            try
            {
                await ReceiveMessage(session);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with client {clientId}: {ex.Message}");
            }
            finally
            {
                _clients.TryRemove(clientId, out _);
                await session.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                session.WebSocket.Dispose();
                Console.WriteLine($"Client {clientId} disconnected. Total clients: {_clients.Count}");
            }
        }

        private async Task ReceiveMessage(Session session)
        {
            byte[] buffer = new byte[1024 * 4];
            while (session.WebSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await session.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (message == "ping")
                {
                    await session.WebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("pong")), WebSocketMessageType.Text, true, CancellationToken.None);
                    return;
                }
                MessageDTO? messageDTO = null;
                try
                {
                    messageDTO = JsonUtil.Deserialize<MessageDTO>(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError("JSON deserialize error: {0}", ex.Message);
                    return;
                }
                if (messageDTO == null)
                {
                    return;
                }
                IMessageHandler? messageHandler = null;
                MethodInfo? methodInfo = null;
                foreach (var h in _messageHandlers)
                {
                    foreach (var m in h.GetType().GetMethods())
                    {
                        foreach (var a in m.GetCustomAttributes())
                        {
                            if (a is OnMessage attr && attr.GetKind() == messageDTO.Kind)
                            {
                                messageHandler = h;
                                methodInfo = m;
                                break;
                            }
                        }
                    }
                }
                if (methodInfo != null)
                {
                    Type parameterInfo = methodInfo.GetParameters()[2].GetModifiedParameterType();
                    var messageData= JsonSerializer.Deserialize(messageDTO.Data, JsonTypeInfo.CreateJsonTypeInfo<RoomJoinDTO>(JsonUtil.JSON_SERIALIZER_OPTIONS));
                    Task? task = methodInfo?.Invoke(messageHandler, new object[] { session, messageDTO, messageData }) as Task;
                    if (task != null) await task;
                }
            }
        }



        public static List<Task> BroadMessage(Session session, Func<string, string> messageFunc)
        {
            var tasks = new List<Task>();
            List<string> clients = _rooms.Where(t => t.Value.Contains(session.ClientId)).FirstOrDefault().Value;
            foreach (var t in clients)
            {
                if (_clients.TryGetValue(t, out var roomClientSession))
                {
                    if (roomClientSession.WebSocket.State == WebSocketState.Open)
                    {
                        string message = messageFunc.Invoke(t);
                        tasks.Add(roomClientSession.WebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None));
                    }
                }
            }
            return tasks;
        }
    }
}
