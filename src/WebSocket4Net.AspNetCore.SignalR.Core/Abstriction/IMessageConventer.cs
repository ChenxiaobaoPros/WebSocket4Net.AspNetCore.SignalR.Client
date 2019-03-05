using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Abstriction
{
    public interface IMessageConventer
    {
        string ProtocolName { get; }
        byte[] GetBytes(Message message);
    }
}
