# AzureServiceBus.Client.Bus

A nuget package that simplifies the usage of Azure Service Bus by abstracting the client setup and exposing only the send and receive methods to the user.

The nuget consists of four classes:
  - TopicClientSendOperations
  - TopicClientReceiveOperations
  - QueueClientSendOperations
  - QueueClientReceiveOperations

## Send Classes

### TopicClientSendOperations

The class exposes only two public methods. 

1) Task<bool> SendAsync(string message). This method gets a string as an argument and returns a boolean, according to the send result. If there is any exception that do not trigger the retry mechanism then the exception is caught, logged through serilog and return *false*. **OBS**, there is automatic retry mechanism in "Microsoft.Azure.ServiceBus" for exception bearing a retry flag. The number of retries is set in the constructor with the parameter - maxRetryCountOnSend.
2) Task CloseAsync(). This method closes asynchronously the client.

Class supports two constructors.

The standard one the uses the following arguments
- serviceBusConnectionString (string) - AzureServiceBusConnectionString
- topicName (string) - Topic name
- minimumBackOff (int) - Minimum backoff between retires in seconds. preset to 1 sec
- maximumBackOff (int) - Maximum backoff between retires in seconds. preset to 10 sec
- maxRetryCountOnSend (int) - Maximum number of retries when message fails to be sent

The advanced constructor runs a check on the topic and create or update it configured with custom made preferences passed as arguments in the constructor. This approach allow as to ensure that the topic will be created if it does not exist, and if it exists it will be updated with the settings available in the constructor before one tries to upload/send a message. The following arguments are required:
- serviceBusConnectionString (string) - AzureServiceBusConnectionString
- topicName (string) - Topic name
- minimumBackOff (int) - Minimum backoff between retires in seconds. preset to 1 sec
- maximumBackOff (int) - Maximum backoff between retires in seconds. preset to 10 sec
- maxRetryCountOnSend (int) - Maximum number of retries when message fails to be sent
- tenantId (string) - TenantId. It can be find in the app registration tab in AAD 
- clientId (string) - ClientId. It can be find in the app registration tab in AAD
- clientSecret (string) - ClientSecret. It can be found in App registrations (AAD) - Certificates and Secrets - ClientSecret
- subscriptionId (string) - SubscriptionId
- resourceGroup (string) - ResourceGroup
- namespaceName (string) - Namespace name
- enableExpress (bool) - It keeps the message in memory for a short while before it persists so performance is boosted
- enableBatchOperations (bool) - It persists in local service bus storage many messages as a batch so performance is boosted


### QueueClientSendOperations

The class exposes only two public methods. 

1) Task<bool> SendAsync(string message). This method gets a string as an argument and returns a boolean, according to the send result. If there is any exception that do not trigger the retry mechanism then the exception is caught, logged through serilog and return *false*. **OBS**, there is automatic retry mechanism in "Microsoft.Azure.ServiceBus" for exception bearing a retry flag. The number of retries is set in the constructor with the parameter - maxRetryCountOnSend.
2) Task CloseAsync(). This method closes asynchronously the client.

Class supports two constructors.

The standard one the uses the following arguments
- connectionString (string) - AzureServiceBusConnectionString
- queueName (string) - Topic name
- minimumBackOff (int) - Minimum backoff between retires in seconds. preset to 1 sec
- maximumBackOff (int) - Maximum backoff between retires in seconds. preset to 10 sec
- maxRetryCountOnSend (int) - Maximum number of retries when message fails to be sent

The advanced constructor runs a check on the topic and create or update it configured with custom made preferences passed as argument sin the constructor. This approach allow as to ensure that the topic will be created if it does not exist, and if it exists it will be updated with the latest preferences passed as arguments before one tries to upload send a message. The following parameters are used:
- connectionString (string) - AzureServiceBusConnectionString
- topicName (string) - Topic name
- minimumBackOff (int) - Minimum backoff between retires in seconds. preset to 1 sec
- maximumBackOff (int) - Maximum backoff between retires in seconds. preset to 10 sec
- maxRetryCountOnSend (int) - Maximum number of retries when message fails to be sent
- tenantId (string) - TenantId. It can be find in the app registration tab in AAD 
- clientId (string) - ClientId. It can be find in the app registration tab in AAD
- clientSecret (string) - ClientSecret. It can be found in App registrations (AAD) - Certificates and Secrets - ClientSecret
- subscriptionId (string) - SubscriptionId
- resourceGroup (string) - ResourceGroup
- lockDurationInSeconds (int) - Amount of seconds that the queue should wait for acknowledgment after the message is passed to a client. If message is not acknowledged then it returns back to the queue ready for processing again
- maxDeliveryCount (int) - Amount of times a message should try to be processed before it ends up in the dead letter queue
- namespaceName (string) - Namespace name
- enableExpress (bool) - It keeps the message in memory for a short while before it persists so performance is boosted
- enableBatchOperations (bool) - It persists in local service bus storage many messages as a batch so performance is boosted

### Suggested usage of the send packages

It is strongly suggested to be used only one instance of the client and keep the connection open through out the application. For this reason, it is strongly suggested the implementation to be injected with a singleton lifetime in the DI container in .net-core applications

```
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new TopicClientSendOperations(...));
            services.AddControllers();
        }
```
OR for Queue
```
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new QueueClientSendOperations(...));
            services.AddControllers();
        }
```

Then in the example below is demonstrated a controller that gets an http call and sends a message to ASB Queue/Topic
```
[ApiController]
    [Route("[controller]")]
    public class SendMessageController : ControllerBase
    {
        private TopicClientSendOperations _client;

        public SendMessageController(TopicClientSendOperations client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var messageSent = await _client.SendAsync("Message sent");
            if (messageSent)
                return StatusCode(200);
            else
                return StatusCode(500);
        }
    }
```
OR for Queue
```
[ApiController]
    [Route("[controller]")]
    public class SendMessageController : ControllerBase
    {
        private QueueClientSendOperations _client;

        public SendMessageController(QueueClientSendOperations client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var messageSent = await _client.SendAsync("Message sent");
            if (messageSent)
                return StatusCode(200);
            else
                return StatusCode(500);
        }
    }
```

## Receive/Subscribe classes

### QueueClientReceiveOperations

The class exposes only two public methods. 

1) void ListenToQueue(Func<string, Task<bool>> messageHandler). This method accepts a delegate that accepts a string and returns a bool. Here, the message handler and all our logic on the received message will be applied. This method starts a new Task that will continue running in the backround. Based on the bool returned by the handler the message will either be Completed and removed from the Subscription OR Abandined and returned back to the queue so it will be reprocessed. If a message is processed 10 times without success then it is automatically moved to the dead letter queue,  except if it is set otherwise by the QueueClientReceiveOperations class.
2) Task CloseAsync(). This method closes asynchronously the client.

The constructor accepts the following arguments
- serviceBusConnectionString (string) - AzureServiceBusConnectionString,
- queueName (string) - Queue name
- prefetchCount (int) - Amount of messages that will be prefetched and ready for processing, so performance is boosted
- autoComplete (bool) - If true, every message will automatically ne removed from the queue as soon as it is grabbed from a client, if false the message will wait for manual acknowledgement,
- maxConcurrentMessages (int) - Amount of messages processed at the same time.


### TopicClientReceiveOperations

The class exposes only two public methods. 

1) void Subscribe(Func<string, Task<bool>> messageHandler). This method accepts a delegate that accepts a string and returns a bool. Here, the message handler and all our logic on the received message will be applied. This method starts a new Task that will continue running in the backround. Based on the bool returned by the handler the message will either be Completed and removed from the Subscription OR Abandined and returned back to the queue so it will be reprocessed. If a message is processed 10 times without success then it is automatically moved to the dead letter queue. The amount of times a message would be processed before it end in the dead letter can be changed through the constructor paramater *maxDeliveryCount*
2) Task CloseAsync(). This method closes asynchronously the client.

The constructor accepts the following arguments
- serviceBusConnectionString (string) - AzureServiceBusConnectionString,
- queueName (string) - Queue name
- prefetchCount (int) - Amount of messages that will be prefetched and ready for processing, so performance is boosted
- autoComplete (bool) - If true, every message will automatically ne removed from the queue as soon as it is grabbed from a client, if false the message will wait for manual acknowledgement,
- maxConcurrentMessages (int) - Amount of messages processed at the same time.



The advanced constructor runs a check on the subscription in a specific topic, if it exists and create or update it, accordingly, configured with custom made preferences passed as arguments in the constructor. This approach allow as to ensure that the subscription will be created if it does not exist, and if it exists it will be updated with the settings available in the constructor before one tries to receive a message. The following arguments are required:
- serviceBusConnectionString (string) - AzureServiceBusConnectionString
- topicName (string) - Topic name
- minimumBackOff (int) - Minimum backoff between retires in seconds. preset to 1 sec
- maximumBackOff (int) - Maximum backoff between retires in seconds. preset to 10 sec
- maxRetryCountOnSend (int) - Maximum number of retries when message fails to be sent
- tenantId (string) - TenantId. It can be find in the app registration tab in AAD 
- clientId (string) - ClientId. It can be find in the app registration tab in AAD
- clientSecret (string) - ClientSecret. It can be found in App registrations (AAD) - Certificates and Secrets - ClientSecret
- subscriptionId (string) - SubscriptionId
- resourceGroup (string) - ResourceGroup
- namespaceName (string) - Namespace name
- enableExpress (bool) - It keeps the message in memory for a short while before it persists so performance is boosted
- enableBatchOperations (bool) - It persists in local service bus storage many messages as a batch so performance is boosted
- subscriptionName (string) - The name of the new subcription on the topic
- prefetchCount (int) - The amount of messages taht will be prefetched, intents to boost performance
- maxDeliveryCount (int) - Amount of times a message should be delivered and Abandoned or timeout before it ends up in dead letter queue
- maxConcurrentMessages (int) - Amount of messages processed simultaneously by the application, intents to boost performance
- lockDurationInSeconds (int) - Wait in seconds before the queue consideres the message timed out, and requeue it
- autocomplete (bool) - When true, it automatiovally removes the message from the queue after delivery, whan false a manual acknowledgement is neede before the message is removed;

### Suggested usage of the send packages
The message receive process is suggested to be used in a worker .net-core project, that will ensure that our application will stay alive an dcontinue to liesten/subscribe to a queue/topic.

The program.cs class in Worker project would be
```
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
                    services.AddSingleton(new QueueClientReceiveOperations(...));
                    services.AddSingleton<IMessageHandler, MessageHandler>();
                    services.AddHostedService<Worker>();
                });
    }
```
The worker class
```
       public class Worker : BackgroundService
    {
        private QueueClientReceiveOperations _client;
        private IMessageHandler _handler;

        public Worker(QueueClientReceiveOperations client, IMessageHandler handler)
        {
            _handler = handler;
            _client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.ListenToQueue(_handler.Handle);
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
```
the IMessageHandler interface
```
    public interface IMessageHandler
    {
        Task<bool> Handle(string message);
    }
```
and the MessageHandler.cs
```
    public class MessageHandler : IMessageHandler
    {
        public async Task<bool> Handle(string message)
        {
            // process message async
            Console.WriteLine($"Message: {message}");

            return true;
        }
    }
```