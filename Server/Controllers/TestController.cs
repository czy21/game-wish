using Microsoft.AspNetCore.Mvc;
using WishServer.Service;

namespace WishServer.Controllers
{
    [Route("test")]
    public class TestController : ControllerBase
    {
        ILogger<WebSocketController> _logger;
        IGameUserService _gameUserService;
        IGameGiftService _gameGiftService;

        public TestController(ILogger<WebSocketController> logger, IGameUserService gameUserService,IGameGiftService gameGiftService)
        {
            _logger = logger;
            _gameUserService = gameUserService;
            _gameGiftService = gameGiftService;
        }

        [HttpGet("db")]
        public async Task test1()
        {
            //await _gameUserService.UpSert();
            await _gameGiftService.UpSert();
        }
    }
}
