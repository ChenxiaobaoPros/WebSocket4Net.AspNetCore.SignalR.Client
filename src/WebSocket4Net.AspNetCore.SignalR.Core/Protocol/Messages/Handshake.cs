
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    public class Handshake : Message
    {
        public Handshake(string protocol)
        {
            this.Protocol = protocol;
        }
        public string Protocol { get; set; }
        public int Version { get; set; } = 1;

        public new object Headers { get; set; }
    }
}
