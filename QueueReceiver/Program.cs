using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureServiceBus.CustomQueueClient.Bus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace QueueReceiver
{
    class Program
    {
        static QueueClient queueClient;
        static async Task Main(string[] args)
        {

            var client = new CustomQueueReceiveClient("Endpoint=sb://stylianoslinux.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/CNuBgPssNjxJ2CDnZFYrEy+cN6zekrwzSzs4aNRkjk=", "testqueue", 1, false, 1);


            var cl = new MessageReceiver("Endpoint=sb://stylianoslinux.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/CNuBgPssNjxJ2CDnZFYrEy+cN6zekrwzSzs4aNRkjk=", "testqueue", ReceiveMode.PeekLock, RetryPolicy.Default, 0);

            client.ListenToQueue(ReadMessageAndDisplay);
            //var listeningToQueue = Task.Factory.StartNew(() =>
            //{
            //    client.ListenToQueue(ReadMessageAndDisplay);
            //}, TaskCreationOptions.LongRunning);



            //queueClient = new QueueClient("Endpoint=sb://stylianoslinux.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/CNuBgPssNjxJ2CDnZFYrEy+cN6zekrwzSzs4aNRkjk=",
            //    "testqueue/$DeadLetterQueue", ReceiveMode.PeekLock, RetryPolicy.Default);

            //RegisterOnMessageHandlerAndReceiveMessages();

            Console.ReadKey();
        }

        private static async Task<bool> ReadMessageAndDisplay(string message)
        {
            var client = new HttpClient();
            var a = await client.GetAsync("http://stylianosgiakoumidis.eu");
            var b = await a.Content.ReadAsStringAsync();

            Console.WriteLine($"This comes from handler. Message body: {message}, extra: {b}");
            return true;
        }

        private static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that will process messages
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
            // to avoid unnecessary exceptions.
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
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
