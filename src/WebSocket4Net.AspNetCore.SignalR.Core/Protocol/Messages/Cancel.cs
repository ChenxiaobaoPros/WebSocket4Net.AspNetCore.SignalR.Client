

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
   public class Cancel : Message
    {
        public Cancel()
        {
            this.Type = 5;
        }
        public string InvocationId { get; set; }
    }
}
