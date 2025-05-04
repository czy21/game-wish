using Microsoft.AspNetCore.Mvc;
using WishServer.Domain;
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

        public TestController(ILogger<WebSocketController> logger, IGameUserService gameUserService, IGameGiftService gameGiftService, IGameRoomService gameRoomService)
        {
            _logger = logger;
            _gameUserService = gameUserService;
            _gameGiftService = gameGiftService;
            _gameRoomService = gameRoomService;
        }

        [HttpGet("db1")]
        public async Task<GameRoomDTO?> test1([FromQuery] string roomId)
        {
            //await _gameUserService.UpSert();
            //await _gameGiftService.UpSert();
            return await _gameRoomService.AggRoom(roomId);
        }

        [HttpGet("db2")]
        public async Task<GameRoomPO?> test2([FromQuery] string roomId)
        {
            return await _gameRoomService.GetOne(roomId);
        }

        [HttpGet("db3")]
        public async Task<GameRoomPO?> test3([FromQuery] string roomId, [FromQuery] string userId)
        {
            await _gameRoomService.SaveOne();
            return await Task.FromResult(GameRoomPO.Empty());
        }
    }
}
