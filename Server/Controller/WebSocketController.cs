using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using WishServer.Annotation;
using WishServer.Extension;
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
                    await session.WebSocket.SendTextAsync("pong");
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
                    ParameterInfo[] methodParamInfos = methodInfo.GetParameters();
                    object?[] methodParams = new object[methodParamInfos.Length];
                    for (int i = 0; i < methodParamInfos.Length; i++)
                    {
                        ParameterInfo parameterInfo = methodParamInfos[i];
                        Type paramType = parameterInfo.ParameterType;
                        if (paramType == session.GetType())
                        {
                            methodParams[i] = session;
                            continue;
                        }
                        if (paramType == messageDTO.GetType())
                        {
                            methodParams[i] = messageDTO;
                            continue;
                        }
                        if (typeof(IMessage).IsAssignableFrom(paramType))
                        {
                            IMessage? messageObj = (IMessage?)JsonUtil.Deserialize(messageDTO.Data, paramType);
                            if (messageObj == null) return;

                            messageObj.Kind = messageDTO.Kind;
                            methodParams[i] = messageObj;
                            continue;
                        }
                    }
                    Task? task = methodInfo?.Invoke(messageHandler, methodParams) as Task;
                    if (task != null) await task;
                }
            }
        }



        public static List<Task> BroadMessage(Session session, Func<string, object> messageFunc)
        {
            var tasks = new List<Task>();
            List<string> clients = _rooms.Where(t => t.Value.Contains(session.ClientId)).FirstOrDefault().Value;
            foreach (var t in clients)
            {
                if (_clients.TryGetValue(t, out var roomClientSession))
                {
                    if (roomClientSession.WebSocket.State == WebSocketState.Open)
                    {
                        object message = messageFunc.Invoke(t);
                        tasks.Add(roomClientSession.WebSocket.SendJsonAsnyc(message));
                    }
                }
            }
            return tasks;
        }
    }
}
