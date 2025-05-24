using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using WishServer.Annotation;
using WishServer.Extension;
using WishServer.Manager;
using WishServer.Model;
using WishServer.Service;
using WishServer.Service.impl;
using WishServer.Util;

namespace WishServer.Controllers;

public class WebSocketController : ControllerBase
{
    public static readonly ConcurrentDictionary<string, Session> ClientIdSessionDict = new();
    private readonly ILogger<WebSocketController> _logger;
    private readonly Dictionary<PlatformEnum, IMessageHandler> _messageHandlerDict;
    private readonly RoomManager _roomManager;

    public WebSocketController(ILogger<WebSocketController> logger, IEnumerable<IMessageHandler> messageHandlers,RoomManager roomManager)
    {
        _logger = logger;
        _messageHandlerDict = messageHandlers.ToDictionary(k => k.GetPlatform(), v => v);
        _roomManager = roomManager;
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
        ClientIdSessionDict.TryAdd(session.ClientId, session);
        if (_messageHandlerDict.TryGetValue((PlatformEnum)platform, out var messageHandler)) await messageHandler.Init(session);
        
        _roomManager.StartRoomConsumer(session.Platform, session.RoomId);
        
        _logger.LogInformation($"Client {session.ClientId} connected. Total clients: {ClientIdSessionDict.Count}");

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
        
        _roomManager.StopRoomConsumer(session.Platform, session.RoomId);
        
        if (messageHandler != null) await messageHandler.Exit(session);
        
        ClientIdSessionDict.TryRemove(session.ClientId, out _);
        await session.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
        session.WebSocket.Dispose();
        _logger.LogInformation($"Client {session.ClientId} disconnected. Total clients: {ClientIdSessionDict.Count}");
    }

    public async Task HandleMessage(IMessageHandler messageHandler, Session session, string message)
    {
        if (string.IsNullOrEmpty(message) || messageHandler == null) return;

        var messageDto = JsonUtil.Deserialize<MessageDTO>(message);
        if (messageDto == null) return;

        var methodInfo = messageHandler
            .GetType()
            .GetMethods()
            .FirstOrDefault(m => m.GetCustomAttributes().Any(a => a is OnMessage attr && attr.GetKind() == messageDto.Kind));

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

                if (paramType == messageDto.GetType())
                {
                    methodParams[i] = messageDto;
                    continue;
                }

                if (!typeof(IMessage).IsAssignableFrom(paramType)) continue;
                
                var messageObj = (IMessage)JsonUtil.Deserialize(messageDto.Data, paramType);
                if (messageObj == null) return;

                messageObj.Kind = messageDto.Kind;
                methodParams[i] = messageObj;
            }

            if (methodInfo.Invoke(messageHandler, methodParams) is Task task) await task;
        }
    }
}