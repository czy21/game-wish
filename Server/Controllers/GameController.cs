using Microsoft.AspNetCore.Mvc;

namespace WishServer.Controllers
{
    [Route("game")]
    public class GameController: ControllerBase
    {

        [HttpPost("getPlayers")]
        public Task GetPlayers()
        {
            return Task.CompletedTask;
        }

        [HttpPost("setPlayers")]
        public Task SetPlayers()
        {
            return Task.CompletedTask;
        }

        [HttpPost("getUser")]
        public Task GetUser()
        {
            return Task.CompletedTask;
        }

        [HttpPost("setGift")]
        public Task SetGift()
        {
            return Task.CompletedTask;
        }

        [HttpPost("round/syncStatus")]
        public Task RoundSyncStatus()
        {
            return Task.CompletedTask;
        }

        
    }
}
