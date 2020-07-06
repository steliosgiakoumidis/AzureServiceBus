using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Serilog;

namespace AzureServiceBus.Client.Bus
{
    public class QueueClientReceiveOperations
    {
        private string _serviceBusConnectionString;
        private string _queueName;
        private int _prefetchCount;
        private bool _autocomplete;
        private int _maxConcurrentMessages;
        private MessageReceiver CustomQueueClient;

        public QueueClientReceiveOperations(string serviceBusConnectionString, string queueName, int prefetchCount, bool autoComplete,
            int maxConcurrentMessages)
        {
            _serviceBusConnectionString = serviceBusConnectionString;
            _queueName = queueName;
            _prefetchCount = prefetchCount;
            _autocomplete = autoComplete;
            _maxConcurrentMessages = maxConcurrentMessages;
            CustomQueueClient = CreateMessageReceiver();
        }

        public void ListenToQueue(Func<string, Task<bool>> messageHandler)
        {
            var task = Task.Run(() =>
            {
                CustomQueueClient.RegisterMessageHandler(
                    async (message, cancellationToken) =>
                    {
                        if (await messageHandler(Encoding.UTF8.GetString(message.Body)))
                            await CustomQueueClient.CompleteAsync(message.SystemProperties.LockToken);
                        else
                            await CustomQueueClient.AbandonAsync(message.SystemProperties.LockToken);
                    },
                    new MessageHandlerOptions(ExceptionReceivedHandler)
                    {
                        AutoComplete = _autocomplete,
                        MaxConcurrentCalls = _maxConcurrentMessages
                    });
            });
        }

        public async Task CloseAsync() => await CustomQueueClient.CloseAsync();

        private MessageReceiver CreateMessageReceiver()
        {
            return new MessageReceiver(_serviceBusConnectionString, _queueName, ReceiveMode.PeekLock, RetryPolicy.Default, _prefetchCount);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Log.Error($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}. Endpoint: {context.Endpoint}" +
                $" Entity Path: {context.EntityPath} Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
