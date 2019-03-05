
namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    public class Message
    {
        public int Type { get; set; } = 6;
        public object Headers { get; set; } = new object();
    }
}
