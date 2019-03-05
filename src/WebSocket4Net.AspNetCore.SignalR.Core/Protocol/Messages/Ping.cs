using WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages;

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    public class Ping : Message
    {
        public static Ping Instance = new Ping();
        public Ping()
        {
            this.Type = 6;
        }
    }
}
