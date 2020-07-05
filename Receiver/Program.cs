using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Receiver
{
    class Program
    {
        static IQueueClient queueClient;

        static async Task Main(string[] args)
        {
            //var client = new CustomTopicReceiveClient()
            //{



            //    //// https://login.microsoftonline.com/ + TenantID from app registration in AAD
            //    //var context = new AuthenticationContext($"https://login.microsoftonline.com/1a98abbf-23f4-4593-9e87-0cc577b3374c");

            //    ////ClientCredential in appRegistration -> 1. clientID, 2) AAD -> app registration -> Certificates&Secrets -> ClientSecret
            //    //var token = await context.AcquireTokenAsync("https://management.azure.com/", new ClientCredential("e5a7d2ab-9d55-4996-9480-0e23248d23ec", "9Ue1y~_H1~.Rzeog8pk1sUrb_lqc~LI4l1"));


            //    //var creds = new TokenCredentials(token.AccessToken);
            //    //var sbClient = new ServiceBusManagementClient(creds)
            //    //{
            //    //    SubscriptionId = "af7b2dac-d8ce-4995-a2e2-e45935ef2b51"
            //    //};
            //    //var topicParam = new SBTopic()
            //    //{
            //    //    DefaultMessageTimeToLive = TimeSpan.FromSeconds(10)
            //    //};
            //    //var oarams = new SBQueue()
            //    //{
            //    //    LockDuration = TimeSpan.FromSeconds(30),
            //    //    MaxDeliveryCount = 5,
            //    //    EnableExpress = true,
            //    //    EnableBatchedOperations = true
            //    //};

            //    //await sbClient.Queues.CreateOrUpdateAsync("LinuxStelios", "stylianoslinux", "testqueue", oarams);


            //    //queueClient = new QueueClient(new ServiceBusConnectionStringBuilder(""));

            //    //RegisterOnMessageHandlerAndReceiveMessages();

            //    //Console.WriteLine("Hello World!");
            //};
        }

        private static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler);
        }

        private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
