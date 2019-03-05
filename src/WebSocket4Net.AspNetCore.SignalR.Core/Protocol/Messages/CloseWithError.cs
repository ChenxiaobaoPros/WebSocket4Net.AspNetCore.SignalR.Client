

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    class CloseWithError: Close
    {   
        public string Error { get; set; }
    }
}
