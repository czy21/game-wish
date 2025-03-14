using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using WishServer.Annotation;
using WishServer.Extension;
using WishServer.Manager;
using WishServer.Model;
using WishServer.Util;

namespace WishServer.Controllers
{
    public class WebSocketController : ControllerBase
    {

        private readonly ILogger<WebSocketController> _logger;
        private readonly IEnumerable<IMessageHandler> _messageHandlers;

        public WebSocketController(ILogger<WebSocketController> logger, IEnumerable<IMessageHandler> messageHandlers)
        {
            _logger = logger;
            _messageHandlers = messageHandlers;
        }

        public static readonly ConcurrentDictionary<string, Session> CLIENT_DICT = new();
        public static readonly ConcurrentDictionary<string, HashSet<string>> ROOM_DICT = new();

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
                WebSocket = webSocket
            };

            CLIENT_DICT.TryAdd(clientId, session);

            byte[] buffer = new byte[1024 * 4];
            while (session.WebSocket.State == WebSocketState.Open)
            {
                try
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
                        continue;
                    }
                    await HandleMessage(session, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error with client {session.ClientId}: {ex.Message}");
                }
            }

            if (session.WebSocket.State != WebSocketState.Open)
            {
                CLIENT_DICT.TryRemove(clientId, out _);
                await session.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                session.WebSocket.Dispose();
                _logger.LogInformation($"Client {clientId} disconnected. Total clients: {CLIENT_DICT.Count}");
            }
        }


        public async Task HandleMessage(Session session, string message)
        {

            MessageDTO? messageDTO = JsonUtil.Deserialize<MessageDTO>(message);
            if (messageDTO == null) return;

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


        public static List<Task> BroadMessage(Session session, Func<string, object> messageFunc)
        {
            var tasks = new List<Task>();
            var clients = ROOM_DICT.Where(t => t.Value.Contains(session.ClientId)).FirstOrDefault().Value;
            foreach (var t in clients)
            {
                if (CLIENT_DICT.TryGetValue(t, out var roomClientSession))
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
