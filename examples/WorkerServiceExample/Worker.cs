using System.Threading;
using System.Threading.Tasks;
using AzureServiceBus.Client.Bus;
using Microsoft.Extensions.Hosting;
using WorkerServiceTest.Handler;

namespace WorkerServiceTest
{
    public class Worker : BackgroundService
    {
        private QueueClientReceiveOperations _client;
        private IMessageHandler _handler;

        public Worker(QueueClientReceiveOperations client, IMessageHandler handler)
        {
            _handler = handler;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.ListenToQueue(_handler.Handle);
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
