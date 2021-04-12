using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Fullcircl.Server.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Fullcircl.Server
{
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        public HomeController(ILogger<HomeController> logger, RedisConnectionService redis)
        {
            _logger = logger;
            _redis = redis;
        }

        private readonly ILogger<HomeController> _logger;
        private readonly RedisConnectionService _redis;

        [HttpGet("redis-cache")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult RedisCache()
        {
            var result = new Dictionary<string, string?>
            {
                { "Message", "A simple example with Azure Cache for Redis on ASP.NET Core." }
            };

            IDatabase cache = _redis.GetDatabase();

            // Perform cache operations using the cache object...

            // Simple PING command
            result.Add("command1", "PING");
            result.Add("command1Result", cache?.Execute(result["command1"])?.ToString());

            // Simple get and put of integral data types into the cache
            result.Add("command2", "GET Message");
            result.Add("command2Result", cache?.StringGet("Message").ToString());

            result.Add("command3", "SET Message \"Hello! The cache is working from ASP.NET Core!\"");
            result.Add("command3Result", cache?.StringSet("Message", "Hello! The cache is working from ASP.NET Core!").ToString());

            // Demonstrate "SET Message" executed as expected...
            result.Add("command4", "GET Message");
            result.Add("command4Result", cache?.StringGet("Message").ToString());

            // Get the client list, useful to see if connection list is growing...
            // Note that this requires allowAdmin=true in the connection string
            result.Add("command5", "CLIENT LIST");
            StringBuilder sb = new StringBuilder();
            var endpoint = (System.Net.DnsEndPoint)_redis.GetEndPoints()[0];
            IServer server = _redis.GetServer(endpoint.Host, endpoint.Port);
            ClientInfo[] clients = server.ClientList();

            sb.AppendLine("Cache response :");
            foreach (ClientInfo client in clients)
            {
                sb.AppendLine(client.Raw);
            }

            result.Add("command5Result", sb.ToString());

            return Ok(result);
        }
    }
}