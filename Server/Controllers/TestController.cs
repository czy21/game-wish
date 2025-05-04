using Microsoft.AspNetCore.Mvc;
using WishServer.Model.DTO;
using WishServer.Service;

namespace WishServer.Controllers
{
    [Route("test")]
    public class TestController : ControllerBase
    {
        ILogger<WebSocketController> _logger;
        IGameUserService _gameUserService;
        IGameGiftService _gameGiftService;
        IGameRoomService _gameRoomService;

        public TestController(ILogger<WebSocketController> logger, IGameUserService gameUserService,IGameGiftService gameGiftService,IGameRoomService gameRoomService)
        {
            _logger = logger;
            _gameUserService = gameUserService;
            _gameGiftService = gameGiftService;
            _gameRoomService = gameRoomService;
        }

        [HttpGet("db")]
        public async Task<GameRoomDTO?> test1([FromQuery]string roomId)
        {
            //await _gameUserService.UpSert();
            //await _gameGiftService.UpSert();
            return await _gameRoomService.AggRoom(roomId);
        }
    }
}
