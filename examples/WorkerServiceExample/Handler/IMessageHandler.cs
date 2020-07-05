using System.Threading.Tasks;

namespace WorkerServiceTest.Handler
{
    public interface IMessageHandler
    {
        Task<bool> Handle(string message);
    }
}
