using System.Collections.Concurrent;
using StackExchange.Redis;
using Sunny.Framework.Cache;
using Sunny.Framework.External.Client;
using Sunny.Framework.External.Client.DY;
using Sunny.Framework.External.Util;
using WishServer.Model;
using WishServer.Model.DTO;
using WishServer.Model.DY;
using WishServer.Repository;
using WishServer.Util;

namespace WishServer.Service.impl;

public class DYPlatformService : AbstractMessageHandler, IMessageHandler
{
    private readonly ILogger<DYPlatformService> _logger;
    
    private readonly IDYClient _dYClient;
    private readonly IDYOAuthClient _dyOAuthClient;
    
    private readonly IDatabase _redisDatabase;
    private static readonly ConcurrentDictionary<string, DYRoomSession> RoomIdSessionDict = new();

    public DYPlatformService(
        ILogger<DYPlatformService> logger,
        IGameAppRepository gameAppRepository,
        RedisDataSource redisDataSource,
        IDYOAuthClient dYoAuthClient,
        IDYClient dyClient
    ) : base(logger, gameAppRepository, redisDataSource.GetInstance("Token").GetDatabase())
    {
        _logger = logger;
        _redisDatabase = redisDataSource.GetDefault().GetDatabase();
        _dyOAuthClient = dYoAuthClient;
        _dYClient = dyClient;
    }

    public override PlatformEnum GetPlatform()
    {
        return PlatformEnum.DY;
    }

    public async Task<string> SignatureReceive(string gameCode, Dictionary<string, object> headers, string rawBody)
    {
        var gameApp = await GetGameApp(gameCode);
        return DYUtil.SignatureReceive(headers, rawBody, gameApp.GameApp.AppSecretPush);
    }

    public override async Task<string> GetAccessToken(string gameCode)
    {
        var gameApp = await GetGameApp(gameCode);

        return await GetCacheValue(((IMessageHandler)this).GetAccessTokenKey(gameCode),
            async () =>
            {
                DYAccessTokenReq req = new()
                {
                    appid = gameApp.GameApp.AppId,
                    secret = gameApp.GameApp.AppSecret,
                    grant_type = "client_credential"
                };
                var res = await _dyOAuthClient.GetAccessToken(req);
                return res.data.access_token;
            }
        );
    }

    public async Task<GameRoomDTO> GetLiveInfo(string gameCode, string token)
    {
        var accessToken = await GetAccessToken(gameCode);

        DYLiveInfoReq param = new() { token = token };

        var res = await _dYClient.GetLiveInfo(param, accessToken);
        GameRoomDTO grDto = new();
        if (res.data?.info?.room_id != null)
        {
            var gameApp = await GetGameApp(gameCode);
            grDto = new GameRoomDTO
            {
                Game = gameApp?.Game,
                GameApp = gameApp?.GameApp,
                RoomId = res.data?.info?.room_id.ToString(),
                AnchorId = res.data?.info?.anchor_open_id,
                AvatarUrl = res.data?.info?.avatar_url,
                Nickname = res.data?.info?.nick_name
            };
            await _redisDatabase.StringSetAsync(((IMessageHandler)this).GetRoomKey(grDto.RoomId), JsonUtil.Serialize(grDto), TimeSpan.FromDays(1));
        }

        grDto.Game = null;
        grDto.GameApp = null;
        return await Task.FromResult(grDto);
    }

    public override async Task Init(Session session)
    {
        RoomIdSessionDict.AddOrUpdate(session.RoomId, new DYRoomSession { Session = session }, (k, v) => v);
        await DoRoomTask(session.RoomId);
    }

    public async Task DoRoomTask(string roomId)
    {
        var roomSession = RoomIdSessionDict[roomId];
        foreach (var t in roomSession.Tasks.Where(t => t.TaskStatus != "SUCCESS"))
        {
            var accessToken = await GetAccessToken(roomSession.Session.GameCode);
            var gameApp = await GetGameApp(roomSession.Session.GameCode);
            var taskRes = await _dYClient.StartTaskPush(
                new DYLiveDataTaskReq
                {
                    appid = gameApp.GameApp.AppId,
                    roomid = roomId,
                    msg_type = t.TaskType
                }, accessToken);
            t.TaskId = taskRes.data.taskid;
            if (!string.IsNullOrEmpty(t.TaskId)) t.TaskStatus = "SUCCESS";
        }
    }

    public override async Task Exit(Session session)
    {
        var removeRooms = RoomIdSessionDict
            .Where(t => t.Value.Session.ClientId == session.ClientId)
            .ToDictionary(k => k.Key, v => v.Value);
        foreach (var r in removeRooms)
        {
            var accessToken = await GetAccessToken(r.Value.Session.GameCode);
            var gameApp = await GetGameApp(r.Value.Session.GameCode);
            foreach (var t in r.Value.Tasks)
                await _dYClient.StopTaskPush(
                    new DYLiveDataTaskReq
                    {
                        appid = gameApp.GameApp.AppId,
                        roomid = r.Key,
                        msg_type = t.TaskType
                    }, accessToken);
            RoomIdSessionDict.TryRemove(r.Key, out _);
        }
    }

    public async Task Ack(string gameCode, string roomId, int ackType, List<Dictionary<string, object>> data)
    {
        var accessToken = await GetAccessToken(gameCode);
        var gameApp = await GetGameApp(gameCode);
        var param = new DYLiveDataAckReq
        {
            app_id = gameApp.GameApp.AppId,
            room_id = roomId,
            ack_type = ackType,
            data = JsonUtil.Serialize(data)
        };
        await _dYClient.Ack(param, accessToken);
    }

    protected override async Task DoPeriodTask()
    {
        foreach (var k in RoomIdSessionDict.Keys)
        {
            await DoRoomTask(k);
        }
    }

    public static HashSet<string> GetObjMsgTypes()
    {
        return ["user_group_push"];
    }

    public static HashSet<string> GetArrMsgTypes()
    {
        return ["live_comment", "live_like", "live_gift"];
    }

    public static HashSet<string> GetPushMsgTypes()
    {
        return [.. GetObjMsgTypes(), .. GetArrMsgTypes()];
    }

    public static HashSet<string> GetAckMsgTypes()
    {
        return [.. GetArrMsgTypes()];
    }
}