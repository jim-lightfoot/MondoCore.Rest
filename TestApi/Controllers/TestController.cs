using Microsoft.AspNetCore.Mvc;

namespace TestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("name")]
        public string Get()
        {
            return "bob";
        }

        [HttpGet]
        [Route("name_timesout")]
        public async Task<string> Get2()
        {
            await Task.Delay(2000);
            return "bob";
        }
    }
}
