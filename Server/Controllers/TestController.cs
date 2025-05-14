using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nacos.V2;
using WishServer.Domain;
using WishServer.Model.DTO;
using WishServer.Service;

namespace WishServer.Controllers;

[Route("test")]
public class TestController : ControllerBase
{
    private readonly IGameRoomService _gameRoomService;

    private readonly AppSetting _settings1;
    private readonly AppSetting _settings2;
    private readonly AppSetting _settings3;
    private readonly IWishUserService _wishUserService;

    private AppSetting _configProperties;
    private IConfiguration _configuration;
    private IGameGiftService _gameGiftService;
    private IGameUserService _gameUserService;
    private ILogger<WebSocketController> _logger;
    private INacosConfigService _nacosConfigService;


    public TestController(ILogger<WebSocketController> logger,
        IGameUserService gameUserService,
        IGameGiftService gameGiftService,
        IGameRoomService gameRoomService,
        IConfiguration configuration,
        INacosConfigService nacosConfigService,
        IOptions<AppSetting> options1,
        IOptionsSnapshot<AppSetting> options2,
        IOptionsMonitor<AppSetting> options3,
        IWishUserService wishUserService
    )
    {
        _logger = logger;
        _gameUserService = gameUserService;
        _gameGiftService = gameGiftService;
        _gameRoomService = gameRoomService;
        _configuration = configuration;

        _configProperties = options1.Value;

        _settings1 = options1.Value;
        _settings2 = options2.Value;
        _settings3 = options3.CurrentValue;

        _nacosConfigService = nacosConfigService;
        _wishUserService = wishUserService;
    }

    [HttpGet("db1")]
    public async Task<List<GameRoomDTO>> test1([FromQuery] string roomId)
    {
        //await _gameUserService.UpSert();
        //await _gameGiftService.UpSert();
        return await _gameRoomService.AggRoom(roomId);
    }

    [HttpGet("db2")]
    public async Task<GameRoomPO> test2([FromQuery] string roomId)
    {
        return await _gameRoomService.GetOne(roomId);
    }

    [HttpGet("db3")]
    public async Task<GameRoomPO> test3([FromQuery] string roomId, [FromQuery] string userId)
    {
        await _gameRoomService.SaveOne();
        return await Task.FromResult(GameRoomPO.Empty());
    }

    [HttpGet("db4")]
    public async Task test4([FromQuery(Name = "id")] long id, [FromQuery(Name = "error")] bool error)
    {
        await _wishUserService.TestTransaction(id, error);
    }

    [HttpGet("settings")]
    public async Task<Dictionary<string, object>> settings()
    {
        return await Task.FromResult(new Dictionary<string, object>
        {
            { "_settings1", _settings1 },
            { "_settings2", _settings2 },
            { "_settings3", _settings3 }
        });
    }
}