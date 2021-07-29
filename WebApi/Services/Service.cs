using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace WebApi.Services
{
    public class Service: ReadingService.ReadingServiceBase
    {
        private readonly ILogger<Service> _logger;

        public Service(ILogger<Service> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<StatusMessage> AddReading(ReadingMessage request, ServerCallContext context)
        {
            return Task.FromResult(new StatusMessage
            {
                Message = $"Successful! Customer = {request.Customer}",
                Success = ReadingStatus.Success
            });
        }

        public override async Task<Empty> SendDiagnostics(IAsyncStreamReader<ReadingMessage> requestStream, ServerCallContext context)
        {
            await Task.Run(async () =>
            {
                await foreach (var reading in requestStream.ReadAllAsync())
                {
                    _logger.LogInformation($"Received reading: {reading}");
                }
            });

            return new Empty();
        }
    }
}