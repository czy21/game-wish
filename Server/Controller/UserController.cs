using Microsoft.AspNetCore.Mvc;

namespace WishServer.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpPost("user/login")]
        public Task<Dictionary<string, object>> Login([FromBody] Dictionary<string, object> param)
        {
            return Task.FromResult(param);
        }
    }
}
