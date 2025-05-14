using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using WishServer.Annotation;
using WishServer.Extension;
using WishServer.Model;
using WishServer.Service;
using WishServer.Util;

namespace WishServer.Controllers;

public class WebSocketController : ControllerBase
{
    public static readonly ConcurrentDictionary<string, Session> CLIENTID_SESION_DICT = new();
    private readonly ILogger<WebSocketController> _logger;
    private readonly Dictionary<PlatformEnum, IMessageHandler> _messageHandlerDict;

    public WebSocketController(ILogger<WebSocketController> logger, IEnumerable<IMessageHandler> messageHandlers)
    {
        _logger = logger;
        _messageHandlerDict = messageHandlers.ToDictionary(k => k.GetPlatform(), v => v);
    }

    [Route("/socket")]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (!HttpContext.Request.Query.TryGetValue("gameCode", out var gameCode)) return;

        if (!HttpContext.Request.Query.TryGetValue("platform", out var platformStr)) return;

        if (!Enum.TryParse(typeof(PlatformEnum), platformStr, out var platform)) return;

        if (!HttpContext.Request.Query.TryGetValue("roomId", out var roomId)) return;

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        Session session = new()
        {
            ClientId = Guid.NewGuid().ToString(),
            ConnectionInfo = HttpContext.Connection,
            WebSocket = webSocket,
            GameCode = gameCode,
            Platform = (PlatformEnum)platform,
            RoomId = roomId
        };
        if (_messageHandlerDict.TryGetValue((PlatformEnum)platform, out var messageHandler)) await messageHandler.Init(session);

        CLIENTID_SESION_DICT.TryAdd(session.ClientId, session);

        _logger.LogInformation($"Client {session.ClientId} connected. Total clients: {CLIENTID_SESION_DICT.Count}");

        var buffer = new byte[1024 * 4];
        while (session.WebSocket.State == WebSocketState.Open)
            try
            {
                var result = await session.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (message == "ping")
                {
                    await session.WebSocket.SendTextAsync("pong");
                    continue;
                }

                await HandleMessage(messageHandler, session, message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error with client {session.ClientId}: {ex.Message}");
            }

        if (messageHandler != null) await messageHandler.Exit(session);

        CLIENTID_SESION_DICT.TryRemove(session.ClientId, out _);
        await session.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
        session.WebSocket.Dispose();
        _logger.LogInformation($"Client {session.ClientId} disconnected. Total clients: {CLIENTID_SESION_DICT.Count}");
    }

    public async Task HandleMessage(IMessageHandler messageHandler, Session session, string message)
    {
        if (string.IsNullOrEmpty(message) || messageHandler == null) return;

        var messageDTO = JsonUtil.Deserialize<MessageDTO>(message);
        if (messageDTO == null) return;

        var methodInfo = messageHandler.GetType()
            .GetMethods()
            .Where(m => m.GetCustomAttributes().Any(a => a is OnMessage attr && attr.GetKind() == messageDTO.Kind))
            .FirstOrDefault();

        if (methodInfo != null)
        {
            var methodParamInfos = methodInfo.GetParameters();
            var methodParams = new object[methodParamInfos.Length];
            for (var i = 0; i < methodParamInfos.Length; i++)
            {
                var parameterInfo = methodParamInfos[i];
                var paramType = parameterInfo.ParameterType;
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
                    var messageObj = (IMessage)JsonUtil.Deserialize(messageDTO.Data, paramType);
                    if (messageObj == null) return;

                    messageObj.Kind = messageDTO.Kind;
                    methodParams[i] = messageObj;
                }
            }

            if (methodInfo?.Invoke(messageHandler, methodParams) is Task task) await task;
        }
    }
}