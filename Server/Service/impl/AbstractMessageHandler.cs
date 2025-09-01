using StackExchange.Redis;
using WishServer.Model;
using WishServer.Model.BO;
using WishServer.Repository;

namespace WishServer.Service.impl;

public abstract class AbstractMessageHandler : BackgroundService, IMessageHandler
{
    private readonly IGameAppRepository _gameAppRepository;
    private readonly ILogger<DYPlatformService> _logger;
    internal readonly IDatabase _redisTokenDatabase;
    internal readonly IDatabase _redisDefaultDatabase;

    protected AbstractMessageHandler(
        ILogger<DYPlatformService> logger,
        IGameAppRepository gameAppRepository,
        IDatabase redisTokenDatabase,
        IDatabase redisDefaultDatabase
    )
    {
        _logger = logger;
        _gameAppRepository = gameAppRepository;
        _redisTokenDatabase = redisTokenDatabase;
        _redisDefaultDatabase = redisDefaultDatabase;
    }

    /// <summary>
    /// 分布式锁设置缓存并获取
    /// </summary>
    /// <param name="key"></param>
    /// <param name="fetchFunc"></param>
    /// <param name="lockExpire">锁过期时间(秒)</param>
    /// <param name="fetchCacheMaxCount">从缓存中获取最大重试次数</param>
    /// <param name="keyExpire">key过期时间(秒)</param>
    /// <param name="keyPreExpire">key预过期时间(秒)</param>
    /// <returns></returns>
    protected async Task<string> GetCacheValue(string key, Func<Task<string>> fetchFunc, long lockExpire = 5, long fetchCacheMaxCount = 5, long keyExpire = 3600, long keyPreExpire = 300)
    {
        key = $"{{{key}}}";
        var expireKey = $"{key}:expire";

        var value = await _redisTokenDatabase.StringGetAsync(key);
        var expireAt = await _redisTokenDatabase.StringGetAsync(expireKey);

        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(expireAt) && DateTimeOffset.Now.ToUnixTimeMilliseconds() < long.Parse(expireAt) - keyPreExpire * 1000)
        {
            return value;
        }

        var lockKey = $"{key}:lock";
        var lockVal = Guid.NewGuid().ToString();
        var lockRet = await _redisTokenDatabase.StringSetAsync(lockKey, lockVal, TimeSpan.FromSeconds(lockExpire), When.NotExists, CommandFlags.None);

        if (lockRet)
        {
            try
            {
                value = await _redisTokenDatabase.StringGetAsync(key);
                expireAt = await _redisTokenDatabase.StringGetAsync(expireKey);
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(expireAt) && DateTimeOffset.Now.ToUnixTimeMilliseconds() < long.Parse(expireAt) - keyPreExpire * 1000)
                {
                    return value;
                }

                var newValue = await fetchFunc();
                if (!string.IsNullOrEmpty(newValue))
                {
                    await _redisTokenDatabase.ScriptEvaluateAsync(
                        """
                        local newToken = ARGV[1]
                        local expireSeconds = tonumber(ARGV[2])
                        local expireTimestamp = tonumber(ARGV[3])
                        redis.call('SET', KEYS[1], newToken, 'EX', expireSeconds)
                        redis.call('SET', KEYS[2], expireTimestamp, 'EX', expireSeconds)
                        return 1
                        """,
                        [key, expireKey],
                        [newValue, keyExpire, DateTimeOffset.Now.ToUnixTimeMilliseconds() + keyExpire * 1000]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Set {Key} cache fail", key);
                await _redisTokenDatabase.ScriptEvaluateAsync("if redis.call('get', KEYS[1]) == ARGV[1] then return redis.call('del', KEYS[1]) else return 0 end", [lockKey], [lockVal]);
            }
        }

        var fetchCacheCount = 0;
        do
        {
            _logger.LogDebug("Getting {Key} from cache; Attempt {FetchCacheCount}", key, fetchCacheCount + 1);
            value = await _redisTokenDatabase.StringGetAsync(key);
            if (!string.IsNullOrEmpty(value)) break;
            await Task.Delay(200 * fetchCacheCount);
            fetchCacheCount++;
        } while (fetchCacheCount < fetchCacheMaxCount);

        return value;
    }

    public abstract PlatformEnum GetPlatform();
    public abstract Task<string> GetAccessToken(string gameCode);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            _logger.LogInformation("{Platform} Push Check Task is running.", GetPlatform().ToString());
            await DoPeriodTask();
        }
    }

    protected abstract Task DoPeriodTask();

    public abstract Task Init(Session session);
    public abstract Task Exit(Session session);

    private string GetRoomConnectKey(Session session)
    {
        return string.Join(":", "game", "ROOM_CONNECT", GetPlatform(), session.RoomId);
    }
    
    protected async Task SetRoomConnect(Session session)
    {
        await _redisDefaultDatabase.StringSetAsync(GetRoomConnectKey(session), session.ConnectionInfo.LocalIpAddress + ":" + session.ConnectionInfo.LocalPort);
    }

    protected async Task DelRoomConnect(Session session)
    {
        await _redisDefaultDatabase.KeyDeleteAsync(GetRoomConnectKey(session));
    }

    protected async Task<GameAppBO> GetGameApp(string gameCode)
    {
        var gameApp = await _gameAppRepository.SelectOneByPlatformAndGameCode(GetPlatform().ToString(), gameCode);
        if (gameApp == null) throw new Exception($"GameApp {gameCode} ${GetPlatform()} not exist");
        return gameApp;
    }
}