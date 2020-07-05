using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Serilog;

namespace AzureServiceBus.CustomQueueClient.Bus
{
    public class CustomQueueReceiveClient
    {
        private string _queueConnectionString;
        private string _queueName;
        private int _prefetchCount;
        private bool _autocomplete;
        private int _maxConcurrentMessages;
        public MessageReceiver QueueClientReceive;

        public CustomQueueReceiveClient(string queueConnectionString, string queueName, int prefetchCount, bool autoComplete,
            int maxConcurrentMessages)
        {
            _queueConnectionString = queueConnectionString;
            _queueName = queueName;
            _prefetchCount = prefetchCount;
            _autocomplete = autoComplete;
            _maxConcurrentMessages = maxConcurrentMessages;
            QueueClientReceive = CreateMessageReceiver();
        }

        private MessageReceiver CreateMessageReceiver()
        {
            return new MessageReceiver(_queueConnectionString, _queueName, ReceiveMode.PeekLock, RetryPolicy.Default, _prefetchCount);
        }

        public void ListenToQueue(Func<string, Task<bool>> messageHandler)
        {
            var task = Task.Run(() =>
            {
                QueueClientReceive.RegisterMessageHandler(
                    async (message, cancellationToken) =>
                    {
                        if (await messageHandler(Encoding.UTF8.GetString(message.Body)))
                            await QueueClientReceive.CompleteAsync(message.SystemProperties.LockToken);
                        else
                            await QueueClientReceive.AbandonAsync(message.SystemProperties.LockToken);
                    },
                    new MessageHandlerOptions(ExceptionReceivedHandler)
                    {
                        AutoComplete = _autocomplete,
                        MaxConcurrentCalls = _maxConcurrentMessages
                    });
            });
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
