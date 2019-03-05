
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    public class StreamItem : Message
    {
        public StreamItem()
        {
            this.Type = 1;
        }
        public string InvocationId { get; set; }
        public int Item { get; set; }
    }
}
