using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WishServer.Model;

namespace WishServer.Controllers
{
    public class WebSocketController : ControllerBase
    {

        private readonly ILogger<UserController> _logger;

        public WebSocketController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        private static readonly ConcurrentDictionary<string, Session> _clients = new();
        private static readonly ConcurrentDictionary<string, string[]> _rooms = new();

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
                ConnectionInfo = HttpContext.Connection,
                WebSocket = webSocket
            };

            _clients.TryAdd(clientId, session);

            try
            {
                await ReceiveMessage(clientId, session);
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

        private async Task ReceiveMessage(string clientId, Session session)
        {
            byte[] buffer = new byte[1024 * 4];
            while (session.WebSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await session.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                await HandleMessage(clientId, session, Encoding.UTF8.GetString(buffer, 0, result.Count));
            }
        }

        private async Task HandleMessage(string clientId, Session session, string message)
        {
            if (message == "ping")
            {
                await session.WebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("pong")), WebSocketMessageType.Text, true, CancellationToken.None);
                return;
            }
            MessageDTO? messageObj;
            try
            {
                messageObj = JsonSerializer.Deserialize<MessageDTO>(message);
            }
            catch (Exception ex)
            {
                _logger.LogError("JSON deserialize error: {0}", ex.Message);
                return;
            }
            if (messageObj == null)
            {
                return;
            }

            switch (messageObj.Kind)
            {
                case MessageKind.ROOM_CREATE:

                    break;
                case MessageKind.ROOM_JOIN:

                    break;
                case MessageKind.ROOM_LEAVE:

                    break;

                default:
                    break;
            }

            var tasks = new List<Task>();
            foreach (KeyValuePair<string, Session> t in _clients)
            {
                if (t.Value.WebSocket.State == WebSocketState.Open)
                {
                    string messagePush = t.Key == clientId ? string.Format("自己 => {0}", message) : string.Format("{0}:{1} => {2}", session.ConnectionInfo.RemoteIpAddress, session.ConnectionInfo.RemotePort, message);
                    tasks.Add(t.Value.WebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(messagePush)), WebSocketMessageType.Text, true, CancellationToken.None));
                }
            }

            await Task.WhenAll(tasks);
        }
    }
}
