using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WishServer.Annotation;
using WishServer.Extension;
using WishServer.Model;

namespace WishServer.Service.impl;

public class ABPlatformService : IMessageHandler
{
    private readonly AppSetting _config;

    private readonly ILogger<DYPlatformService> _logger;

    private readonly ConcurrentDictionary<string, RoomSession> ROOM_SESSION_DICT = new();

    public ABPlatformService(ILogger<DYPlatformService> logger, IOptions<AppSetting> options)
    {
        _logger = logger;
        _config = options.Value;
    }

    public PlatformEnum GetPlatform()
    {
        return PlatformEnum.AB;
    }

    public Task DoRoomTask(string roomId)
    {
        throw new NotImplementedException();
    }

    public Task Init(Session session)
    {
        if (session.RoomId == null) return Task.CompletedTask;
        ROOM_SESSION_DICT.AddOrUpdate(session.RoomId, new RoomSession { Session = session }, (k, v) => v);
        return Task.CompletedTask;
    }

    public Task Exit(Session session)
    {
        var removeRooms = ROOM_SESSION_DICT.Where(t => t.Value.Session.ClientId == session.ClientId).ToDictionary(k => k.Key, v => v.Value);
        foreach (var r in removeRooms) ROOM_SESSION_DICT.TryRemove(r.Key, out _);
        return Task.CompletedTask;
    }

    public Task<string> GetAccessToken(string gameCode)
    {
        return Task.FromResult("");
    }

    public async Task SendMessages(string roomId, string msgType, List<Dictionary<string, object>> param)
    {
        if (roomId == null || param == null) return;

        if (ROOM_SESSION_DICT.TryGetValue(roomId, out var roomSession))
            await roomSession.Session.WebSocket.SendJsonAsnyc(
                new Dictionary<string, object>
                {
                    ["msgType"] = msgType,
                    ["msg"] = param
                });
    }

    [OnMessage(MessageKind.ROOM_REPORT)]
    public async Task HandleRoomReport(Session session, MessageDTO messageDTO)
    {
    }
}