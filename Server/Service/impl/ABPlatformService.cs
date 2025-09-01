using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Sunny.Framework.Cache;
using WishServer.Annotation;
using WishServer.Extension;
using WishServer.Model;
using WishServer.Repository;

namespace WishServer.Service.impl;

public class ABPlatformService : AbstractMessageHandler,IMessageHandler
{

    private readonly ILogger<DYPlatformService> _logger;
    private readonly ConcurrentDictionary<string, RoomSession> RoomIdSessionDict = new();
    
    private readonly AppSetting _config;

    public ABPlatformService(ILogger<DYPlatformService> logger,
        IGameAppRepository gameAppRepository,
        RedisDataSource redisDataSource,
        IOptions<AppSetting> options
    ) : base(logger, gameAppRepository, redisDataSource.GetInstance("Token").GetDatabase(),redisDataSource.GetDefault().GetDatabase())
    {
        _logger = logger;
        _config = options.Value;
    }

    public override PlatformEnum GetPlatform()
    {
        return PlatformEnum.AB;
    }

    public Task DoRoomTask(string roomId)
    {
        throw new NotImplementedException();
    }

    protected override Task DoPeriodTask()
    {
        throw new NotImplementedException();
    }

    public override async Task Init(Session session)
    {
        if (session.RoomId == null) return;
        RoomIdSessionDict.AddOrUpdate(session.RoomId, new RoomSession { Session = session }, (k, v) => v);
        await SetRoomConnect(session);
    }

    public override async Task Exit(Session session)
    {
        var removeRooms = RoomIdSessionDict.Where(t => t.Value.Session.ClientId == session.ClientId).ToDictionary(k => k.Key, v => v.Value);
        foreach (var r in removeRooms)
        {
            RoomIdSessionDict.TryRemove(r.Key, out _);
            await DelRoomConnect(r.Value.Session);
        };
    }

    public override Task<string> GetAccessToken(string gameCode)
    {
        return Task.FromResult("");
    }

    public async Task SendMessages(string roomId, string msgType, List<Dictionary<string, object>> param)
    {
        if (roomId == null || param == null) return;

        if (RoomIdSessionDict.TryGetValue(roomId, out var roomSession))
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