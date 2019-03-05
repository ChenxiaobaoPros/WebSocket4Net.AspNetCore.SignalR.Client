

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    class FaildCompletion : Completion
    {
        public FaildCompletion(string invocationId,string errorMessage)
        {
            this.InvocationId = invocationId;
            this.Error = errorMessage;
        }
        public string Error { get; set; }
    }
}
