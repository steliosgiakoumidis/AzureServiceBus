using System;
using System.Threading.Tasks;

namespace WorkerServiceTest.Handler
{
    public class MessageHandler : IMessageHandler
    {
        public async Task<bool> Handle(string message)
        {
            // process message async
            Console.WriteLine($"Message: {message}");

            return true;
        }
    }
}
