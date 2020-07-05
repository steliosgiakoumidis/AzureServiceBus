using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureServiceBus.Client.Bus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SendMessageExample.Controllers
{
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
        public async Task Get()
        {
            var messageSent = await _client.SendAsync("Message sent");
        }
    }
}
