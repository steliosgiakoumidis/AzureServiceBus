using System;
using System.Text;
using System.Threading.Tasks;
using AzureServiceBus.CustomQueueClient.Bus;
using Microsoft.Azure.Management.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace AzureServiceBus
{
    //class Program
    //{

    //    using System;
    //using System.Text;
    //using System.Threading;
    //using System.Threading.Tasks;
    //using Microsoft.Azure.ServiceBus;

    class Program
    {
        const string ServiceBusConnectionString = "Endpoint=sb://stylianoslinux.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/CNuBgPssNjxJ2CDnZFYrEy+cN6zekrwzSzs4aNRkjk=";
        const string TopicName = "testtopic";
        static ITopicClient topicClient;

        public static async Task Main(string[] args)
        {
            var client = new CustomTopicSendClient("Endpoint=sb://stylianoslinux.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/CNuBgPssNjxJ2CDnZFYrEy+cN6zekrwzSzs4aNRkjk=", "testtopic22", 1, 10, 5, "1a98abbf-23f4-4593-9e87-0cc577b3374c", "e5a7d2ab-9d55-4996-9480-0e23248d23ec", "9Ue1y~_H1~.Rzeog8pk1sUrb_lqc~LI4l1", "af7b2dac-d8ce-4995-a2e2-e45935ef2b51", "LinuxStelios", "stylianoslinux", false, true);

            for (int i = 0; i < 2; i++)
            {
                await client.SendAsync($"This is message {i}");
            }

            //const int numberOfMessages = 1;
            //topicClient = new TopicClient(ServiceBusConnectionString, TopicName);

            //Console.WriteLine("======================================================");
            //Console.WriteLine("Press ENTER key to exit after sending all the messages.");
            //Console.WriteLine("======================================================");

            //// Send messages.
            //await SendMessagesAsync(numberOfMessages);

            //Console.ReadKey();

            //await topicClient.CloseAsync();
        }

        static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the topic
                    string messageBody = $"Message {i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Write the body of the message to the console
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the topic
                    await topicClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }
    }
    //static async Task Main(string[] args)
    //{



    //    var a = new QueueClient(new ServiceBusConnectionStringBuilder("connString"), ReceiveMode.PeekLock, RetryPolicy.Default);
    //    //var cc = new TopicClient("");


    //    await a.SendAsync(new Message());

    //    var b = new MessageSender(new ServiceBusConnectionStringBuilder("connString"));

    //    Console.WriteLine("Hello World!");
    //}
}

