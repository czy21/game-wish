using StackExchange.Redis;
using WishServer.Model;
using WishServer.Model.BO;
using WishServer.Repository;

namespace WishServer.Service.impl;

public abstract class AbstractMessageHandler : IMessageHandler
{
    private readonly IGameAppRepository _gameAppRepository;
    private readonly ILogger<DYPlatformService> _logger;
    private readonly IDatabase _redisDatabase;

    protected AbstractMessageHandler(
        ILogger<DYPlatformService> logger,
        IGameAppRepository gameAppRepository,
        IDatabase redisDatabase
    )
    {
        _logger = logger;
        _gameAppRepository = gameAppRepository;
        _redisDatabase = redisDatabase;
    }

    public abstract PlatformEnum GetPlatform();
    public abstract Task<string> GetAccessToken(string gameCode);

    public abstract Task DoRoomTask(string roomId);

    public abstract Task Init(Session session);
    public abstract Task Exit(Session session);

    protected async Task<GameAppBO> GetGameApp(string gameCode)
    {
        var gameApp = await _gameAppRepository.SelectOneByPlatformAndGameCode(GetPlatform().ToString(), gameCode);
        if (gameApp == null) throw new Exception($"GameApp {gameCode} ${GetPlatform()} not exist");
        return gameApp;
    }
}