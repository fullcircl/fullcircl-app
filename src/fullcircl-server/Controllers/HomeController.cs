using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Fullcircl.Server
{
    [Route("[controller]")]
    public class HomeController : ControllerBase
    {
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            
            if (Configuration == null)
            {
                lock(_syncRoot)
                {
                    if (Configuration == null)
                    {
                        Configuration = configuration;
                    }
                }
            }
        }

        private static readonly object _syncRoot = new {};

        private readonly ILogger<HomeController> _logger;
        private static IConfiguration? Configuration { get; set; }

        private const string SecretName = "RedisConnection";

        private static long lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
        private static DateTimeOffset firstErrorTime = DateTimeOffset.MinValue;
        private static DateTimeOffset previousErrorTime = DateTimeOffset.MinValue;

        private static readonly object reconnectLock = new object();

        // In general, let StackExchange.Redis handle most reconnects,
        // so limit the frequency of how often ForceReconnect() will
        // actually reconnect.
        public static TimeSpan ReconnectMinFrequency => TimeSpan.FromSeconds(60);

        // If errors continue for longer than the below threshold, then the
        // multiplexer seems to not be reconnecting, so ForceReconnect() will
        // re-create the multiplexer.
        public static TimeSpan ReconnectErrorThreshold => TimeSpan.FromSeconds(30);

        public static int RetryMaxAttempts => 5;

        private static Lazy<ConnectionMultiplexer> lazyConnection = CreateConnection();

        [HttpGet("redis-cache")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult RedisCache()
        {
            var result = new Dictionary<string, string?>
            {
                { "Message", "A simple example with Azure Cache for Redis on ASP.NET Core." }
            };

            IDatabase cache = GetDatabase();

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
            var endpoint = (System.Net.DnsEndPoint)GetEndPoints()[0];
            IServer server = GetServer(endpoint.Host, endpoint.Port);
            ClientInfo[] clients = server.ClientList();

            sb.AppendLine("Cache response :");
            foreach (ClientInfo client in clients)
            {
                sb.AppendLine(client.Raw);
            }

            result.Add("command5Result", sb.ToString());

            return Ok(result);
        }

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        private static Lazy<ConnectionMultiplexer> CreateConnection()
        {
            return new Lazy<ConnectionMultiplexer>(() =>
            {
                string cacheConnection = Configuration[SecretName];
                return ConnectionMultiplexer.Connect(cacheConnection);
            });
        }

        private static void CloseConnection(Lazy<ConnectionMultiplexer> oldConnection)
        {
            if (oldConnection == null)
                return;

            try
            {
                oldConnection.Value.Close();
            }
            catch (Exception)
            {
                // Example error condition: if accessing oldConnection.Value causes a connection attempt and that fails.
            }
        }

        /// <summary>
        /// Force a new ConnectionMultiplexer to be created.
        /// NOTES:
        ///     1. Users of the ConnectionMultiplexer MUST handle ObjectDisposedExceptions, which can now happen as a result of calling ForceReconnect().
        ///     2. Don't call ForceReconnect for Timeouts, just for RedisConnectionExceptions or SocketExceptions.
        ///     3. Call this method every time you see a connection exception. The code will:
        ///         a. wait to reconnect for at least the "ReconnectErrorThreshold" time of repeated errors before actually reconnecting
        ///         b. not reconnect more frequently than configured in "ReconnectMinFrequency"
        /// </summary>
        public static void ForceReconnect()
        {
            var utcNow = DateTimeOffset.UtcNow;
            long previousTicks = Interlocked.Read(ref lastReconnectTicks);
            var previousReconnectTime = new DateTimeOffset(previousTicks, TimeSpan.Zero);
            TimeSpan elapsedSinceLastReconnect = utcNow - previousReconnectTime;

            // If multiple threads call ForceReconnect at the same time, we only want to honor one of them.
            if (elapsedSinceLastReconnect < ReconnectMinFrequency)
                return;

            lock (reconnectLock)
            {
                utcNow = DateTimeOffset.UtcNow;
                elapsedSinceLastReconnect = utcNow - previousReconnectTime;

                if (firstErrorTime == DateTimeOffset.MinValue)
                {
                    // We haven't seen an error since last reconnect, so set initial values.
                    firstErrorTime = utcNow;
                    previousErrorTime = utcNow;
                    return;
                }

                if (elapsedSinceLastReconnect < ReconnectMinFrequency)
                    return; // Some other thread made it through the check and the lock, so nothing to do.

                TimeSpan elapsedSinceFirstError = utcNow - firstErrorTime;
                TimeSpan elapsedSinceMostRecentError = utcNow - previousErrorTime;

                bool shouldReconnect =
                    elapsedSinceFirstError >= ReconnectErrorThreshold // Make sure we gave the multiplexer enough time to reconnect on its own if it could.
                    && elapsedSinceMostRecentError <= ReconnectErrorThreshold; // Make sure we aren't working on stale data (e.g. if there was a gap in errors, don't reconnect yet).

                // Update the previousErrorTime timestamp to be now (e.g. this reconnect request).
                previousErrorTime = utcNow;

                if (!shouldReconnect)
                    return;

                firstErrorTime = DateTimeOffset.MinValue;
                previousErrorTime = DateTimeOffset.MinValue;

                Lazy<ConnectionMultiplexer> oldConnection = lazyConnection;
                CloseConnection(oldConnection);
                lazyConnection = CreateConnection();
                Interlocked.Exchange(ref lastReconnectTicks, utcNow.UtcTicks);
            }
        }

        // In real applications, consider using a framework such as
        // Polly to make it easier to customize the retry approach.
        private static T BasicRetry<T>(Func<T> func)
        {
            int reconnectRetry = 0;
            int disposedRetry = 0;

            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception ex) when (ex is RedisConnectionException || ex is SocketException)
                {
                    reconnectRetry++;
                    if (reconnectRetry > RetryMaxAttempts)
                        throw;
                    ForceReconnect();
                }
                catch (ObjectDisposedException)
                {
                    disposedRetry++;
                    if (disposedRetry > RetryMaxAttempts)
                        throw;
                }
            }
        }

        public static IDatabase GetDatabase()
        {
            return BasicRetry(() => Connection.GetDatabase());
        }

        public static System.Net.EndPoint[] GetEndPoints()
        {
            return BasicRetry(() => Connection.GetEndPoints());
        }

        public static IServer GetServer(string host, int port)
        {
            return BasicRetry(() => Connection.GetServer(host, port));
        }
    }
}