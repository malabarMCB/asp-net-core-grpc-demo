using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using WebApi.Services;

namespace WorkerClient
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private ReadingService.ReadingServiceClient _readingServiceClient;
        private ReadingService.ReadingServiceClient Client 
        {
            get
            {
                if (_readingServiceClient == null)
                {
                    var channel = GrpcChannel.ForAddress("https://localhost:5001");

                    _readingServiceClient = new ReadingService.ReadingServiceClient(channel);
                }

                return _readingServiceClient;
            }
        }

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var counter = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                if (counter % 10 == 0)
                {
                    _logger.LogInformation("Sending diagnostics");
                    var stream = Client.SendDiagnostics();
                    for (int i = 0; i < 5; i++)
                    {
                        await stream.RequestStream.WriteAsync(new ReadingMessage
                        {
                            Customer = 5,
                            ReadingTime = Timestamp.FromDateTime(DateTime.UtcNow),
                            ReadingValue = i
                        });
                    }

                    await stream.RequestStream.CompleteAsync();
                }

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var result = await Client.AddReadingAsync(new ReadingMessage
                {
                    Customer = 2,
                    ReadingValue = 33,
                    ReadingTime = Timestamp.FromDateTime(DateTime.UtcNow)
                });

                if (result.Success == ReadingStatus.Success)
                {
                    _logger.LogInformation("Successfully send");
                }
                else
                {
                    _logger.LogError($"Unsuccessful response: {result.Success}");
                }

                await Task.Delay(3000, stoppingToken);
            }
        }
    }
}
