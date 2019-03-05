
namespace WebSocket4Net.AspNetCore.SignalR.Core.Abstriction
{
    public interface IReceivedMessageHandlerProvider
    {
        IReceivedMessageHandler GetHandler(string message);
    }
}
