using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using AzureServiceBus.CustomQueueClient.Bus;
using Microsoft.Azure.ServiceBus.Core;

namespace QueueSend
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var client = new CustomQueueSendClient("Endpoint=sb://stylianoslinux.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/CNuBgPssNjxJ2CDnZFYrEy+cN6zekrwzSzs4aNRkjk=", "testqueue2", 3, "1a98abbf-23f4-4593-9e87-0cc577b3374c", "e5a7d2ab-9d55-4996-9480-0e23248d23ec", "9Ue1y~_H1~.Rzeog8pk1sUrb_lqc~LI4l1", "af7b2dac-d8ce-4995-a2e2-e45935ef2b51", "LinuxStelios", "stylianoslinux", 30, 5, true, true);

            for (int i = 0; i < 2; i++)
            {
                await client.SendAsync($"This is message no {i}");
            }
        }
    }
}
