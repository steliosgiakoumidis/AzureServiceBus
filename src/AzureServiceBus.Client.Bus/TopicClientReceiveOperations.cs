using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ServiceBus;
using Microsoft.Azure.Management.ServiceBus.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Serilog;

namespace AzureServiceBus.Client.Bus
{
    public class TopicClientReceiveOperations
    {
        private string _serviceBusConnectionString;
        private string _topicName;
        private string _tenantId;
        private string _clientId;
        private string _clientSecret;
        private string _subscriptionId;
        private string _resourceGroup;
        private string _namespaceName;
        private string _subscriptionName;
        private bool _enableBatchOperations;
        private int _prefetchCount;
        private bool _autocomplete;
        private int _maxConcurrentMessages;
        private int _maxDeliveryCount;
        private TimeSpan? _lockDurationInSeconds;
        public MessageReceiver CustomTopicClient;

        public TopicClientReceiveOperations(string serviceBusConnectionString, string topicName, string subscriptionName, int prefetchCount, bool autocomplete, int maxConcurrentMessages)
        {
            _serviceBusConnectionString = serviceBusConnectionString;
            _topicName = topicName;
            _subscriptionName = subscriptionName;
            _prefetchCount = prefetchCount;
            _autocomplete = autocomplete;
            _maxConcurrentMessages = maxConcurrentMessages;
            CustomTopicClient = CreateTopicClient();
        }

        public TopicClientReceiveOperations(string serviceBusConnectionString, string topicName, string tenantId, string clientId, string clientSecret, string subscriptionId, string resourceGroup, string namespaceName, string subscriptionName, bool enableBatchOperations, int maxDeliveryCount, int prefetchCount, bool autocomplete, int maxConcurrentMessages, int lockDurationInhSeconds)
        {
            _serviceBusConnectionString = serviceBusConnectionString;
            _topicName = topicName;
            _tenantId = tenantId;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _subscriptionId = subscriptionId;
            _resourceGroup = resourceGroup;
            _namespaceName = namespaceName;
            _subscriptionName = subscriptionName;
            _enableBatchOperations = enableBatchOperations;
            _prefetchCount = prefetchCount;
            _autocomplete = autocomplete;
            _maxDeliveryCount = maxDeliveryCount;
            _maxConcurrentMessages = maxConcurrentMessages;
            _lockDurationInSeconds = TimeSpan.FromSeconds(lockDurationInhSeconds);

            EnsureTopicSubscription().Wait();
            CustomTopicClient = CreateTopicClient();
        }

        private async Task EnsureTopicSubscription()
        {
            var context = new AuthenticationContext($"https://login.microsoftonline.com/{_tenantId}");
            var token = await context.AcquireTokenAsync("https://management.azure.com/", new ClientCredential(_clientId, _clientSecret));
            var creds = new TokenCredentials(token.AccessToken);
            var sbClient = new ServiceBusManagementClient(creds) { SubscriptionId = _subscriptionId };
            var subscriptionParameters = new SBSubscription()
            {
                LockDuration = _lockDurationInSeconds,
                MaxDeliveryCount = _maxDeliveryCount,
                EnableBatchedOperations = _enableBatchOperations
            };

            await sbClient.Subscriptions.CreateOrUpdateAsync(_resourceGroup, _namespaceName,
                _topicName, _subscriptionName, subscriptionParameters);
        }

        private MessageReceiver CreateTopicClient()
        {
            return new MessageReceiver(_serviceBusConnectionString, _topicName, ReceiveMode.PeekLock, RetryPolicy.Default, _prefetchCount);
        }

        public void Subscribe(Func<string, Task<bool>> messageHandler)
        {
            var task = Task.Run(() =>
            {
                CustomTopicClient.RegisterMessageHandler(
                    async (message, cancellationToken) =>
                    {
                        if (await messageHandler(Encoding.UTF8.GetString(message.Body)))
                            await CustomTopicClient.CompleteAsync(message.SystemProperties.LockToken);
                        else
                            await CustomTopicClient.AbandonAsync(message.SystemProperties.LockToken);
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
