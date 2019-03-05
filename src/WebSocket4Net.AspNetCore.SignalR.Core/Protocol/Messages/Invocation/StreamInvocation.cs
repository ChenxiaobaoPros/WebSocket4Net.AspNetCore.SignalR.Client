

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    public class StreamInvocation : Message
    {
        public StreamInvocation()
        {
            this.Type = 4;
        }
        public string InvocationId { get; set; }
        public string Target { get; set; }
        public object[] Arguments { get; set; }
    }
}
