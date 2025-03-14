using Microsoft.AspNetCore.Mvc;
using WishServer.Model;

namespace WishServer.Controllers
{
    [Route("message")]
    public class MessageController: Controller
    {

        private readonly ILogger<MessageController> _logger;

        public MessageController(ILogger<MessageController> logger)
        {
            _logger = logger;
        }

        [HttpPost("send")]
        public Task<MessageDTO> Send([FromBody] MessageDTO param)
        {
            return Task.FromResult(param);
        }
    }
}
