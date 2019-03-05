
using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    class Close : Message
    {
        public Close()
        {
            this.Type = 7;
        }
        public string InvocationId { get; set; }
    }
}
