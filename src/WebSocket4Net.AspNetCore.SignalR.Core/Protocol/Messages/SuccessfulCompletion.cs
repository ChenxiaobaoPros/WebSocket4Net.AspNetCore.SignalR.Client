

namespace WebSocket4Net.AspNetCore.SignalR.Core.Protocol.Messages
{
    class SuccessfulCompletion : Completion
    {
        public SuccessfulCompletion(string invocationId,string result="")
        {
            this.InvocationId = invocationId;
            this.Result = result;
        }
        public object Result { get; set; } = "";
    }
}
