

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    public class CompletionWithBothErrorAndResult : Completion
    {
        public object Result { get; set; }
        public string Error { get; set; }
    }
}
