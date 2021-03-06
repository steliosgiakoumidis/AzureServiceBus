﻿using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ServiceBus;
using Microsoft.Azure.Management.ServiceBus.Models;
using Microsoft.Azure.ServiceBus;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Serilog;

namespace AzureServiceBus.Client.Bus
{
    public class TopicClientSendOperations
    {
        private string _serviceBusConnectionString;
        private string _topicName;
        private int _minimumBackOff;
        private int _maximumBackOff;
        private int _maxRetryCountOnSend;
        private string _tenantId;
        private string _clientId;
        private string _clientSecret;
        private string _subscriptionId;
        private string _resourceGroup;
        private string _namespaceName;
        private bool _enableExpress;
        private bool _enableBatchOperations;

        private ITopicClient CustomTopicClient;

        public TopicClientSendOperations(string serviceBusConnectionString, string topicName, int maxRetryCountOnSend, int minimumBackOff = 1, int maximumBackOff = 10)
        {
            _serviceBusConnectionString = serviceBusConnectionString;
            _topicName = topicName;
            _minimumBackOff = minimumBackOff;
            _maximumBackOff = maximumBackOff;
            _maxRetryCountOnSend = maxRetryCountOnSend;
            CustomTopicClient = CreateTopicClient();
        }

        public TopicClientSendOperations(string serviceBusConnectionString, string topicName, int maxRetryCountOnSend, string tenantId, string clientId, string clientSecret, string subscriptionId, string resourceGroup, string namespaceName, bool enableExpress, bool enableBatchOperations, int minimumBackOff = 1, int maximumBackOff = 10)
        {
            _serviceBusConnectionString = serviceBusConnectionString;
            _topicName = topicName;
            _minimumBackOff = minimumBackOff;
            _maximumBackOff = maximumBackOff;
            _maxRetryCountOnSend = maxRetryCountOnSend;
            _tenantId = tenantId;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _subscriptionId = subscriptionId;
            _resourceGroup = resourceGroup;
            _namespaceName = namespaceName;
            _enableExpress = enableExpress;
            _enableBatchOperations = enableBatchOperations;

            EnsureTopic().Wait();
            CustomTopicClient = CreateTopicClient();
        }

        public async Task<bool> SendAsync(string message)
        {
            try
            {
                await CustomTopicClient.SendAsync(new Message(Encoding.UTF8.GetBytes(message)));

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"An exception occured when send message to azure service bus: {ex}");

                return false;
            }
        }

        public async Task CloseAsync() => await CustomTopicClient.CloseAsync();

        private async Task EnsureTopic()
        {
            var context = new AuthenticationContext($"https://login.microsoftonline.com/{_tenantId}");
            var token = await context.AcquireTokenAsync("https://management.azure.com/", new ClientCredential(_clientId, _clientSecret));
            var creds = new TokenCredentials(token.AccessToken);
            var sbClient = new ServiceBusManagementClient(creds) { SubscriptionId = _subscriptionId };
            var topicParams = new SBTopic()
            {
                EnableExpress = _enableExpress,
                EnableBatchedOperations = _enableBatchOperations
            };

            await sbClient.Topics.CreateOrUpdateAsync(_resourceGroup, _namespaceName, _topicName, topicParams);
        }

        private ITopicClient CreateTopicClient()
        {
            var retryPolicy = new RetryExponential(
                minimumBackoff: TimeSpan.FromSeconds(_minimumBackOff),
                maximumBackoff: TimeSpan.FromSeconds(_maximumBackOff),
                maximumRetryCount: _maxRetryCountOnSend
                );
            return new TopicClient(_serviceBusConnectionString, _topicName, retryPolicy);
        }
    }
}
