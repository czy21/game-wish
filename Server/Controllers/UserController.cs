using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WishServer.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly ConfigProperties _config;

        public UserController(ILogger<UserController> logger, IOptions<ConfigProperties> options)
        {
            _logger = logger;
            _config = options.Value;
        }

        [HttpPost("user/login")]
        public Task<Dictionary<string, object>> Login([FromBody] Dictionary<string, object> param, [FromHeader(Name = "X-Token")] string xToken)
        {
            return Task.FromResult(param);
        }
    }
}
