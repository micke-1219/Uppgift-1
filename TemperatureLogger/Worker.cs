using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TemperatureLogger
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private HttpClient _client;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _client = new HttpClient();
            _logger.LogInformation("The service has been started.");
            return base.StartAsync(cancellationToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _client.Dispose();
            _logger.LogInformation("The service has been stopped.");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await _client.GetAsync("https://api.openweathermap.org/data/2.5/onecall?lat=59.33&lon=18.06&units=metric&exclude=minutely,hourly,daily&appid=40dfe6c9de91a5e6e0f2dee18b3b533e");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<CurrentTemperatureModel>(result);
                    _logger.LogInformation($"The temperature in Stockholm is {data.current.temp} degrees Celcius.");
                    if (data.current.temp >= 15)
                    {
                        _logger.LogInformation("The temperature is above 15 degrees Celcius, you don't need a jacket!");
                    }
                }
                else
                {
                    _logger.LogInformation("The request failed.");
                }
                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}
