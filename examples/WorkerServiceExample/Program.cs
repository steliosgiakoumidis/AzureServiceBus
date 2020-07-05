using AzureServiceBus.Client.Bus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkerServiceTest.Handler;

namespace WorkerServiceTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(new QueueClientReceiveOperations("Endpoint=sb://stylianoslinux.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/CNuBgPssNjxJ2CDnZFYrEy+cN6zekrwzSzs4aNRkjk=", "testqueue", 1, false, 1));
                    services.AddSingleton<IMessageHandler, MessageHandler>();
                    services.AddHostedService<Worker>();
                });
    }
}
