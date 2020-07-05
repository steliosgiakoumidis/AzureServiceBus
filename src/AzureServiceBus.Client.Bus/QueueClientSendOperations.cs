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
    public class QueueClientSendOperations
    {
        private string _queueConnectionString;
        private string _queueName;
        private int _maxRetryCountOnSend;
        private int _minimumBackOff;
        private string _tenantId;
        private string _clientId;
        private string _clientSecret;
        private string _subscriptionId;
        private string _resourceGroup;
        private string _namespaceName;
        private int _maximumBackOff;
        private TimeSpan? _lockDurationInSeconds;
        private int _maxDeliveryCount;
        private bool _enableExpress;
        private bool _enableBatchOperations;
        public MessageSender CustomQueueClient;

        public QueueClientSendOperations(string queueConnectionString, string queueName, int maxRetryCountOnSend,
            int minimumBackOffInSeconds = 1, int maximumBackOffInSeconds = 10)
        {
            _queueConnectionString = queueConnectionString;
            _queueName = queueName;
            _minimumBackOff = minimumBackOffInSeconds;
            _maximumBackOff = maximumBackOffInSeconds;
            _maxRetryCountOnSend = maxRetryCountOnSend;
            CustomQueueClient = CreateMessageSender();
        }

        public QueueClientSendOperations(string queueConnectionString, string queueName, int maxRetryCountOnSend,
            string tenantId, string clientId, string clientSecret, string subscriptionId,
            string resourceGroupName, string namespaceName,
            int lockDurationInSeconds, int maxDeliveryCount,
            bool enableExpress, bool enableBatchOperations,
            int minimumBackOffInSeconds = 1, int maximumBackOffInSeconds = 10)
        {
            _queueConnectionString = queueConnectionString;
            _queueName = queueName;
            _tenantId = tenantId;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _subscriptionId = subscriptionId;
            _resourceGroup = resourceGroupName;
            _namespaceName = namespaceName;
            _minimumBackOff = minimumBackOffInSeconds;
            _maximumBackOff = maximumBackOffInSeconds;
            _maxRetryCountOnSend = maxRetryCountOnSend;
            _lockDurationInSeconds = TimeSpan.FromSeconds(lockDurationInSeconds);
            _maxDeliveryCount = maxDeliveryCount;
            _enableExpress = enableExpress;
            _enableBatchOperations = enableBatchOperations;

            EnsureQueue().Wait();
            CustomQueueClient = CreateMessageSender();
        }

        private async Task EnsureQueue()
        {
            var context = new AuthenticationContext($"https://login.microsoftonline.com/{_tenantId}");
            var token = await context.AcquireTokenAsync("https://management.azure.com/", new ClientCredential(_clientId, _clientSecret));
            var creds = new TokenCredentials(token.AccessToken);
            var sbClient = new ServiceBusManagementClient(creds) { SubscriptionId = _subscriptionId };
            var queueParams = new SBQueue()
            {
                LockDuration = _lockDurationInSeconds,
                MaxDeliveryCount = _maxDeliveryCount,
                EnableExpress = _enableExpress,
                EnableBatchedOperations = _enableBatchOperations
            };

            await sbClient.Queues.CreateOrUpdateAsync(_resourceGroup, _namespaceName, _queueName, queueParams);
        }

        private MessageSender CreateMessageSender()
        {
            var retryPolicy = new RetryExponential(
                minimumBackoff: TimeSpan.FromSeconds(_minimumBackOff),
                maximumBackoff: TimeSpan.FromSeconds(_maximumBackOff),
                maximumRetryCount: _maxRetryCountOnSend
                );

            return new MessageSender(_queueConnectionString, _queueName, retryPolicy);
        }

        public async Task<bool> SendAsync(string message)
        {
            try
            {
                await CustomQueueClient.SendAsync(new Message(Encoding.UTF8.GetBytes(message)));

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"An exception occured when sending message to azure service bus: {ex}");

                return false;
            }
        }
    }
}
