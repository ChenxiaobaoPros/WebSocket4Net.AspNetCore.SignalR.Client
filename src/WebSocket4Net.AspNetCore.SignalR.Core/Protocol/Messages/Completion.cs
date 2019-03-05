
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    public class Completion : Message
    {
        public Completion()
        {
            this.Type = 3;
        }
        public string InvocationId { get; set; }
    }
}
